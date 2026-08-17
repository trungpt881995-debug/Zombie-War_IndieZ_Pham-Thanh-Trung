using UnityEngine;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.View
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SoldierGroupView : MonoBehaviour, ISoldierGroupView
    {
        private const string SoldierGroupRootName = "SoldierGroupRoot";

        [Header("Grounding")]
        [SerializeField]
        private float gravity = -25f;

        [SerializeField]
        private float groundedStickVelocity = -2f;

        private CharacterController _characterController;
        private Transform _cachedTransform;
        private Transform _soldierGroupRoot;
        private Transform _movementRoot;
        private float _verticalVelocity;

        public SoldierPoint Position
        {
            get
            {
                Vector3 p = MovementRoot.position;

                return new SoldierPoint(
                    p.x,
                    p.y,
                    p.z);
            }
        }

        private Transform CachedTransform
        {
            get
            {
                if (_cachedTransform == null)
                {
                    _cachedTransform = transform;
                }

                return _cachedTransform;
            }
        }

        private Transform SoldierGroupRoot
        {
            get
            {
                if (_soldierGroupRoot == null)
                {
                    ResolveHierarchy();
                }

                return _soldierGroupRoot;
            }
        }

        private Transform MovementRoot
        {
            get
            {
                if (_movementRoot == null)
                {
                    ResolveHierarchy();
                }

                return _movementRoot;
            }
        }

        private bool CharacterControllerLivesOnMovementRoot =>
            CachedTransform == MovementRoot;

        private void Awake()
        {
            _cachedTransform = transform;
            _characterController =
                GetComponent<CharacterController>();

            ResolveHierarchy();
            NormalizeSoldierGroupRoot();
        }

        /// <summary>
        /// Teleports the whole Soldier Group while preserving the authored hierarchy:
        ///
        /// SoldierGroup               <- world/movement root
        /// └── SoldierGroupRoot       <- must stay local identity
        ///     └── Soldier_01..04
        ///
        /// The SoldierGroupView component may live either on SoldierGroup or on
        /// SoldierGroupRoot. The hierarchy is resolved by the explicit
        /// "SoldierGroupRoot" GameObject name, not by the component location.
        /// </summary>
        public void ResetVerticalVelocity()
        {
            _verticalVelocity = 0f;
        }

        public void Teleport(
            Vector3 worldPosition)
        {
            ResolveHierarchy();

            bool controllerWasEnabled =
                _characterController != null &&
                _characterController.enabled;

            if (controllerWasEnabled)
            {
                _characterController.enabled = false;
            }

            MovementRoot.position =
                worldPosition;

            NormalizeSoldierGroupRoot();

            ResetVerticalVelocity();

            if (controllerWasEnabled)
            {
                _characterController.enabled = true;
            }
        }

        public void Move(
            in SoldierMovementStep movement,
            float deltaTime)
        {
            if (_characterController == null)
            {
                return;
            }

            if (float.IsNaN(deltaTime) ||
                float.IsInfinity(deltaTime) ||
                deltaTime <= 0f)
            {
                return;
            }

            ResolveHierarchy();

            if (_characterController.isGrounded &&
                _verticalVelocity < 0f)
            {
                _verticalVelocity =
                    groundedStickVelocity;
            }
            else
            {
                _verticalVelocity +=
                    gravity * deltaTime;
            }

            Vector3 velocity =
                new Vector3(
                    movement.VelocityX,
                    _verticalVelocity,
                    movement.VelocityZ);

            Vector3 requestedDisplacement =
                velocity * deltaTime;

            if (CharacterControllerLivesOnMovementRoot)
            {
                // SoldierGroupView + CharacterController are already on SoldierGroup.
                _characterController.Move(
                    requestedDisplacement);

                NormalizeSoldierGroupRoot();
                return;
            }

            // CharacterController is on SoldierGroupRoot while SoldierGroup is its parent.
            // Let CharacterController solve collisions on the child, then transfer the
            // actual world displacement to SoldierGroup and restore child local identity.
            Vector3 before =
                CachedTransform.position;

            _characterController.Move(
                requestedDisplacement);

            Vector3 actualDisplacement =
                CachedTransform.position - before;

            MovementRoot.position +=
                actualDisplacement;

            NormalizeSoldierGroupRoot();
        }

        private void ResolveHierarchy()
        {
            Transform current =
                CachedTransform;

            Transform resolvedGroupRoot = null;

            if (current.name == SoldierGroupRootName)
            {
                resolvedGroupRoot = current;
            }
            else
            {
                resolvedGroupRoot =
                    FindDescendantByName(
                        current,
                        SoldierGroupRootName);
            }

            // Fallback for unusual placement: walk upward as well.
            if (resolvedGroupRoot == null)
            {
                Transform cursor =
                    current.parent;

                while (cursor != null)
                {
                    if (cursor.name == SoldierGroupRootName)
                    {
                        resolvedGroupRoot = cursor;
                        break;
                    }

                    cursor = cursor.parent;
                }
            }

            _soldierGroupRoot =
                resolvedGroupRoot;

            if (_soldierGroupRoot != null &&
                _soldierGroupRoot.parent != null)
            {
                // The parent of SoldierGroupRoot is the actual world-space group.
                _movementRoot =
                    _soldierGroupRoot.parent;
            }
            else
            {
                // Safe legacy fallback when no named hierarchy exists.
                _movementRoot =
                    current;
            }
        }

        private void NormalizeSoldierGroupRoot()
        {
            Transform groupRoot =
                SoldierGroupRoot;

            if (groupRoot == null ||
                groupRoot == MovementRoot)
            {
                return;
            }

            groupRoot.localPosition =
                Vector3.zero;

            groupRoot.localRotation =
                Quaternion.identity;

            groupRoot.localScale =
                Vector3.one;
        }

        private static Transform FindDescendantByName(
            Transform parent,
            string targetName)
        {
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child =
                    parent.GetChild(i);

                if (child.name == targetName)
                {
                    return child;
                }

                Transform nested =
                    FindDescendantByName(
                        child,
                        targetName);

                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }
    }
}
