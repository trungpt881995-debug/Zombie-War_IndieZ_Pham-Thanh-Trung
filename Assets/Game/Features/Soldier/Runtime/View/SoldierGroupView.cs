using UnityEngine;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.View
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SoldierGroupView : MonoBehaviour, ISoldierGroupView
    {
        [Header("Grounding")]
        [SerializeField] private float gravity = -25f;

        [SerializeField] private float groundedStickVelocity = -2f;

        private CharacterController _characterController;
        private Transform _cachedTransform;
        private float _verticalVelocity;

        public SoldierPoint Position
        {
            get
            {
                Vector3 p = CachedTransform.position;

                return new SoldierPoint(p.x, p.y, p.z);
            }
        }

        private Transform CachedTransform
        {
            get
            {
                if (_cachedTransform == null)
                    _cachedTransform = transform;

                return _cachedTransform;
            }
        }

        private void Awake()
        {
            _cachedTransform = transform;
            _characterController = GetComponent<CharacterController>();
        }

        public void Move(in SoldierMovementStep movement,float deltaTime)
        {
            if (_characterController == null)
                return;

            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) ||
                deltaTime <= 0f)
            {
                return;
            }

            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = groundedStickVelocity;
            }
            else
            {
                _verticalVelocity += gravity * deltaTime;
            }

            Vector3 velocity = new Vector3( movement.VelocityX, _verticalVelocity, movement.VelocityZ);

            _characterController.Move(velocity * deltaTime);
        }
    }
}
