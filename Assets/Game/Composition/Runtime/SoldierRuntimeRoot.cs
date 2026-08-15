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
using ZombieWar.Features.Soldier.View;
using ZombieWar.Features.VFX.Ports;
using ZombieWar.Integration.Audio.Soldier;
using ZombieWar.Integration.Boss;
using ZombieWar.Integration.Feedback.Soldier;
using ZombieWar.Integration.GameState.Soldier;
using ZombieWar.Integration.Level.Soldier;
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
        private IDisposable _gameLevelStartedSubscription;
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

            _runtime.Tick(Time.deltaTime);
        }

        public void ResetSharedHealth()
        {
            _sharedHealth?.ResetHealth();
        }

        private void OnGameLevelStarted(
            GameLevelStartedEvent evt)
        {
            _sharedHealth?.ResetHealth();
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
            }

            _zombieAttackBinding?.Unbind();
            _bossAttackBinding?.Unbind();

            _runtime = null;
            _sharedHealth = null;
            IsInitialized = false;
        }
    }
}
