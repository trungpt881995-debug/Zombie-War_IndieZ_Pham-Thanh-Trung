using System;
using GeneralCore.Architecture;
using GameplayCore.Damage;
using UnityEngine;
using VContainer;
using ZombieWar.Features.Health.Controller;
using ZombieWar.Features.Health.Factories;
using ZombieWar.Features.Level.Events;
using ZombieWar.Features.Soldier.Config;
using ZombieWar.Features.Soldier.Controller;
using ZombieWar.Features.Soldier.Domain;
using ZombieWar.Features.Soldier.Factories;
using ZombieWar.Features.Soldier.Formation;
using ZombieWar.Features.Soldier.Ports;
using ZombieWar.Features.Soldier.View;
using ZombieWar.Features.VFX.Ports;
using ZombieWar.Integration.Audio.Soldier;
using ZombieWar.Integration.Boss;
using ZombieWar.Integration.Feedback.Soldier;
using ZombieWar.Integration.GameState.Soldier;
using ZombieWar.Integration.Level.Soldier;
using ZombieWar.Integration.UI.Health;
using ZombieWar.Integration.VFX.Soldier;
using ZombieWar.Integration.Zombie;

namespace ZombieWar.Composition
{
    /// <summary>
    /// Scene-owned composition root for the concrete Soldier Group runtime.
    /// Pure gameplay objects are created through the existing Soldier/Health factories;
    /// this MonoBehaviour only owns scene references, lifecycle and cross-feature binding.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SoldierRuntimeRoot : MonoBehaviour
    {
        private const int RequiredSoldierViewCount = 4;
        private const string SoldierGroupRootName = "SoldierGroupRoot";

        [Header("Configuration")]
        [SerializeField]
        private SoldierConfig soldierConfig;

        [SerializeField]
        private SoldierGroupConfig soldierGroupConfig;

        [Header("Scene Views")]
        [SerializeField]
        private SoldierGroupView soldierGroupView;

        [SerializeField]
        private SoldierView[] soldierViews =
            new SoldierView[RequiredSoldierViewCount];

        [Header("Optional Presentation Anchor")]
        [Tooltip("Optional component implementing IVFXAnchor, normally TransformVFXAnchor on the Soldier Group root.")]
        [SerializeField]
        private MonoBehaviour damageVfxAnchorBehaviour;

        private SoldierGroupController _runtime;
        private HealthController _sharedHealth;
        private ILevelSoldierBinding _levelBinding;
        private IGameStateSoldierBinding _gameStateBinding;
        private ISoldierVFXAnchorBinding _vfxBinding;
        private IFeedbackSoldierBinding _feedbackBinding;
        private IAudioSoldierBinding _audioBinding;
        private IZombieAttackBinding _zombieAttackBinding;
        private IBossAttackBinding _bossAttackBinding;
        private IUIHealthBinding _uiHealthBinding;
        private ISoldierGroupInputBuffer _inputBuffer;
        private IDisposable _gameLevelStartedSubscription;
        private Transform _formationRoot;
        private Transform _worldGroupRoot;
        private CharacterController _groupCharacterController;
        private SoldierDirection _lastMovementDirection;
        private float _moveRotationDegreesPerSecond;
        private bool _hasLastMovementDirection;
        private bool _vfxAnchorBound;

        public bool IsInitialized { get; private set; }
        public ISoldierGroupRuntime Runtime => _runtime;
        public HealthController SharedHealth => _sharedHealth;

        public void Initialize(IObjectResolver resolver)
        {
            if (IsInitialized)
            {
                return;
            }

            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            ValidateReferences();

            ResolveSoldierGroupHierarchy();
            LockFormationRootLocalIdentity();

            ISoldierGroupFactory groupFactory =
                resolver.Resolve<ISoldierGroupFactory>();

            var viewPorts =
                new ISoldierView[RequiredSoldierViewCount];

            for (int i = 0; i < viewPorts.Length; i++)
            {
                viewPorts[i] = soldierViews[i];
            }

            SoldierSettings settings =
                soldierConfig.CreateSettings();

            _moveRotationDegreesPerSecond =
                settings.MoveRotationDegreesPerSecond;

            _inputBuffer =
                resolver.Resolve<ISoldierGroupInputBuffer>();

            IFormationProvider formationProvider =
                soldierGroupConfig.CreateFormationProvider();

            // Create only after Zombie/Boss/Animation roots have subscribed to
            // SoldierAddedEvent. SoldierGroupController publishes the Level-1
            // SoldierAddedEvent during construction.
            _runtime =
                groupFactory.Create(
                    soldierGroupView,
                    viewPorts,
                    in settings,
                    formationProvider);

            IHealthFactory healthFactory =
                resolver.Resolve<IHealthFactory>();

            _sharedHealth =
                healthFactory.Create(
                    _runtime.GroupId,
                    soldierGroupConfig.SharedMaxHealth);

            BindGameplay(resolver);
            BindPresentation(resolver);

            _gameLevelStartedSubscription =
                resolver.Resolve<IEventSubscriber>()
                    .Subscribe<GameLevelStartedEvent>(OnGameLevelStarted);

            IsInitialized = true;
        }

        private void Update()
        {
            if (!IsInitialized || _runtime == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            UpdateMovementFacing(deltaTime);
            _runtime.Tick(deltaTime);
        }

        private void UpdateMovementFacing(
            float deltaTime)
        {
            if (_inputBuffer == null ||
                _runtime == null ||
                !_runtime.GameplayEnabled ||
                float.IsNaN(deltaTime) ||
                float.IsInfinity(deltaTime) ||
                deltaTime <= 0f)
            {
                return;
            }

            SoldierMoveInput input =
                _inputBuffer.Current;

            if (input.HasInput)
            {
                float x = input.X;
                float z = input.Y;
                float sqrMagnitude =
                    (x * x) + (z * z);

                if (sqrMagnitude > 0.000001f)
                {
                    float inverseLength =
                        1f / Mathf.Sqrt(sqrMagnitude);

                    _lastMovementDirection =
                        new SoldierDirection(
                            x * inverseLength,
                            0f,
                            z * inverseLength);

                    _hasLastMovementDirection = true;
                }
            }

            // Keep finishing the smooth turn after the joystick is released.
            // This also lets newly activated Soldiers inherit the last group-facing
            // direction without rotating the formation/camera root itself.
            if (!_hasLastMovementDirection)
                return;

            int activeCount = Mathf.Min(
                _runtime.ActiveSoldierCount,
                soldierViews.Length);

            for (int i = 0; i < activeCount; i++)
            {
                SoldierView soldierView =
                    soldierViews[i];

                if (soldierView == null ||
                    !soldierView.gameObject.activeInHierarchy)
                {
                    continue;
                }

                soldierView.SetMovementFacing(
                    in _lastMovementDirection,
                    _moveRotationDegreesPerSecond,
                    deltaTime);
            }
        }

        private void LateUpdate()
        {
            if (!IsInitialized)
            {
                return;
            }

            // Hard lock the real formation root after normal gameplay, Animator and
            // CharacterController updates. This prevents any later transform write
            // from leaving SoldierGroupRoot at an offset such as (-10, 0, -10).
            LockFormationRootLocalIdentity();
        }

        public void ResetSharedHealth()
        {
            _sharedHealth?.ResetHealth();
        }

        private void OnGameLevelStarted(
            GameLevelStartedEvent evt)
        {
            _sharedHealth?.ResetHealth();
            LockFormationRootLocalIdentity();
        }

        /// <summary>
        /// Hard-coded scene-edge teleport used by the current Map02 transition.
        /// The actual formation root is derived from the four SoldierView references
        /// owned by this runtime, so no FindFirstObjectByType&lt;SoldierGroupView&gt; guess
        /// is involved.
        /// </summary>
        public void TeleportGroup(
            Vector3 worldPosition)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    "SoldierRuntimeRoot must be initialized before teleporting the group.");
            }

            ResolveSoldierGroupHierarchy();

            if (_worldGroupRoot == null)
            {
                throw new InvalidOperationException(
                    "SoldierRuntimeRoot could not resolve the world SoldierGroup root.");
            }

            bool controllerWasEnabled =
                _groupCharacterController != null &&
                _groupCharacterController.enabled;

            if (controllerWasEnabled)
            {
                _groupCharacterController.enabled = false;
            }

            Vector3 oldWorldPosition =
                _worldGroupRoot.position;

            _worldGroupRoot.position =
                worldPosition;

            LockFormationRootLocalIdentity();
            soldierGroupView.ResetVerticalVelocity();

            if (controllerWasEnabled)
            {
                _groupCharacterController.enabled = true;
            }

            Debug.Log(
                $"[SoldierRuntimeRoot] TeleportGroup " +
                $"WorldRoot='{_worldGroupRoot.name}' {oldWorldPosition} -> {worldPosition}, " +
                $"FormationRoot='{_formationRoot.name}', " +
                $"FormationLocal={_formationRoot.localPosition}.",
                this);
        }

        private void ResolveSoldierGroupHierarchy()
        {
            Transform nearestCommon =
                FindNearestCommonAncestor();

            if (nearestCommon == null)
            {
                throw new InvalidOperationException(
                    "SoldierRuntimeRoot could not resolve a common formation root " +
                    "for the four SoldierView references.");
            }

            Transform namedRoot =
                FindSharedNamedAncestor(
                    SoldierGroupRootName);

            _formationRoot =
                namedRoot != null
                    ? namedRoot
                    : nearestCommon;

            if (_formationRoot.name == SoldierGroupRootName &&
                _formationRoot.parent != null)
            {
                _worldGroupRoot =
                    _formationRoot.parent;
            }
            else
            {
                _worldGroupRoot =
                    _formationRoot;
            }

            _groupCharacterController =
                soldierGroupView.GetComponent<CharacterController>();
        }

        private Transform FindNearestCommonAncestor()
        {
            Transform cursor =
                soldierViews[0].transform.parent;

            while (cursor != null)
            {
                if (ContainsAllSoldierViews(cursor))
                {
                    return cursor;
                }

                cursor =
                    cursor.parent;
            }

            return null;
        }

        private Transform FindSharedNamedAncestor(
            string targetName)
        {
            Transform cursor =
                soldierViews[0].transform.parent;

            while (cursor != null)
            {
                if (string.Equals(
                        cursor.name,
                        targetName,
                        StringComparison.Ordinal) &&
                    ContainsAllSoldierViews(cursor))
                {
                    return cursor;
                }

                cursor =
                    cursor.parent;
            }

            return null;
        }

        private bool ContainsAllSoldierViews(
            Transform candidate)
        {
            for (int i = 0; i < soldierViews.Length; i++)
            {
                Transform soldier =
                    soldierViews[i].transform;

                if (soldier != candidate &&
                    !soldier.IsChildOf(candidate))
                {
                    return false;
                }
            }

            return true;
        }

        private void LockFormationRootLocalIdentity()
        {
            if (_formationRoot == null)
            {
                ResolveSoldierGroupHierarchy();
            }

            if (_formationRoot == null ||
                _formationRoot == _worldGroupRoot)
            {
                return;
            }

            _formationRoot.localPosition =
                Vector3.zero;

            _formationRoot.localRotation =
                Quaternion.identity;

            _formationRoot.localScale =
                Vector3.one;
        }

        private void BindGameplay(IObjectResolver resolver)
        {
            _levelBinding =
                resolver.Resolve<ILevelSoldierBinding>();

            _levelBinding.Bind(_runtime);

            _gameStateBinding =
                resolver.Resolve<IGameStateSoldierBinding>();

            _gameStateBinding.Bind(_runtime);

            _zombieAttackBinding =
                resolver.Resolve<IZombieAttackBinding>();

            _zombieAttackBinding.BindSharedSoldierGroup(
                _sharedHealth);

            _bossAttackBinding =
                resolver.Resolve<IBossAttackBinding>();

            _bossAttackBinding.BindSharedSoldierGroup(
                _sharedHealth);
        }

        private void BindPresentation(IObjectResolver resolver)
        {
            _feedbackBinding =
                resolver.Resolve<IFeedbackSoldierBinding>();

            _feedbackBinding.Bind(_runtime.GroupId);

            _audioBinding =
                resolver.Resolve<IAudioSoldierBinding>();

            _audioBinding.Bind(_runtime.GroupId);

            // The concrete Shared Health is scene-owned, so bind it to the
            // persistent UI integration only after Soldier runtime creation.
            _uiHealthBinding =
                resolver.Resolve<IUIHealthBinding>();

            _uiHealthBinding.Bind(
                _runtime.GroupId,
                _sharedHealth);

            _vfxBinding =
                resolver.Resolve<ISoldierVFXAnchorBinding>();

            if (damageVfxAnchorBehaviour == null)
            {
                return;
            }

            IVFXAnchor anchor =
                damageVfxAnchorBehaviour as IVFXAnchor;

            if (anchor == null)
            {
                throw new InvalidOperationException(
                    "Damage VFX Anchor Behaviour must implement IVFXAnchor.");
            }

            _vfxBinding.Bind(
                _runtime.GroupId,
                anchor);

            _vfxAnchorBound = true;
        }

        private void ValidateReferences()
        {
            RequireReference(
                soldierConfig,
                nameof(soldierConfig));

            RequireReference(
                soldierGroupConfig,
                nameof(soldierGroupConfig));

            RequireReference(
                soldierGroupView,
                nameof(soldierGroupView));

            if (soldierViews == null ||
                soldierViews.Length != RequiredSoldierViewCount)
            {
                throw new InvalidOperationException(
                    "SoldierRuntimeRoot requires exactly four SoldierView references.");
            }

            for (int i = 0; i < soldierViews.Length; i++)
            {
                if (soldierViews[i] == null)
                {
                    throw new InvalidOperationException(
                        $"SoldierView at slot {i} is not assigned.");
                }
            }
        }

        private static void RequireReference<T>(
            T reference,
            string fieldName)
            where T : UnityEngine.Object
        {
            if (reference != null)
            {
                return;
            }

            throw new InvalidOperationException(
                $"SoldierRuntimeRoot requires '{fieldName}' to be assigned.");
        }

        private void OnDestroy()
        {
            _gameLevelStartedSubscription?.Dispose();
            _gameLevelStartedSubscription = null;

            if (_runtime != null)
            {
                _levelBinding?.Unbind(_runtime);
                _gameStateBinding?.Unbind(_runtime);

                if (_vfxAnchorBound)
                {
                    _vfxBinding?.Unbind(_runtime.GroupId);
                }

                _feedbackBinding?.Unbind(_runtime.GroupId);
                _audioBinding?.Unbind(_runtime.GroupId);
                _uiHealthBinding?.Unbind(_runtime.GroupId);
            }

            _zombieAttackBinding?.Unbind();
            _bossAttackBinding?.Unbind();

            _runtime = null;
            _sharedHealth = null;
            _uiHealthBinding = null;
            _inputBuffer = null;
            _formationRoot = null;
            _worldGroupRoot = null;
            _groupCharacterController = null;
            _lastMovementDirection = SoldierDirection.Zero;
            _moveRotationDegreesPerSecond = 0f;
            _hasLastMovementDirection = false;
            IsInitialized = false;
        }
    }
}
