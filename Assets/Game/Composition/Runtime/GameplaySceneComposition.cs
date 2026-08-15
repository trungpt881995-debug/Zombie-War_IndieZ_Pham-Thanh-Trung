using System;
using GeneralCore.Architecture;
using GameplayCore.Time;
using UnityEngine;
using VContainer;
using ZombieWar.Bootstrap;
using ZombieWar.Features.Boss.Factories;
using ZombieWar.Features.Boss.Services;
using ZombieWar.Features.Camera.Ports;
using ZombieWar.Features.Camera.Services;
using ZombieWar.Features.Camera.Unity.Runtime;
using ZombieWar.Features.GameState.Services;
using ZombieWar.Features.GameState.Unity.Runtime;
using ZombieWar.Features.Health.Factories;
using ZombieWar.Features.Level.Services;
using ZombieWar.Features.Level.Unity.Runtime;
using ZombieWar.Features.Map.Services;
using ZombieWar.Features.Map.Unity.Runtime;
using ZombieWar.Features.Projectile.Factories;
using ZombieWar.Features.Projectile.Unity.Runtime;
using ZombieWar.Features.Score.Services;
using ZombieWar.Features.Score.Unity.Runtime;
using ZombieWar.Features.Spawn.Ports;
using ZombieWar.Features.Spawn.Services;
using ZombieWar.Features.Spawn.Unity.Runtime;
using ZombieWar.Features.Targeting.Registry;
using ZombieWar.Features.VFX.Services;
using ZombieWar.Features.VFX.Unity.Runtime;
using ZombieWar.Features.Weapon.Services;
using ZombieWar.Features.Zombie.Factories;
using ZombieWar.Integration.Boss;
using ZombieWar.Integration.Boss.Unity;
using ZombieWar.Integration.GameState.Runtime;
using ZombieWar.Integration.Weapon;
using ZombieWar.Integration.Weapon.Unity;
using ZombieWar.Integration.Soldier.Animation;
using ZombieWar.Integration.Soldier.Animation.Unity;
using ZombieWar.Integration.Zombie;
using ZombieWar.Integration.Zombie.Unity;

namespace ZombieWar.Composition
{
    /// <summary>
    /// Scene composition root for ZombieWar_Gameplay.
    ///
    /// Responsibilities:
    /// - Resolve pure/runtime services from the persistent GameLifetimeScope.
    /// - Bind or initialize scene-specific Unity RuntimeRoot components.
    /// - Reapply the current GameState after all scene gates are available.
    ///
    /// It creates the scene-owned Soldier Group only after every consumer of
    /// SoldierAddedEvent (Zombie, Boss and Soldier animation) is initialized.
    /// Starting a Game Level, spawning and map loading remain game-flow concerns.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplaySceneComposition : MonoBehaviour
    {
        [Header("Lifecycle")]
        [SerializeField]
        private bool bindOnStart = true;

        [SerializeField]
        private bool logSuccessfulBinding;

        [Header("World")]
        [SerializeField]
        private MapRuntimeRoot mapRuntimeRoot;

        [SerializeField]
        private CameraRuntimeRoot cameraRuntimeRoot;

        [Header("Gameplay Runtime Roots")]
        [SerializeField]
        private LevelRuntimeRoot levelRuntimeRoot;

        [SerializeField]
        private ScoreRuntimeRoot scoreRuntimeRoot;

        [SerializeField]
        private ProjectileRuntimeRoot projectileRuntimeRoot;

        [SerializeField]
        private WeaponRuntimeRoot weaponRuntimeRoot;

        [SerializeField]
        private ZombieRuntimeRoot zombieRuntimeRoot;

        [SerializeField]
        private BossRuntimeRoot bossRuntimeRoot;

        [SerializeField]
        private SpawnRuntimeRoot spawnRuntimeRoot;

        [SerializeField]
        private GameStateRuntimeRoot gameStateRuntimeRoot;

        [Header("Presentation Runtime Roots")]
        [SerializeField]
        private VFXRuntimeRoot vfxRuntimeRoot;

        [SerializeField]
        private SoldierAnimationRuntimeRoot soldierAnimationRuntimeRoot;

        [Header("Soldier Runtime")]
        [SerializeField]
        private SoldierRuntimeRoot soldierRuntimeRoot;

        private bool _isBound;

        public bool IsBound => _isBound;

        private void Start()
        {
            if (bindOnStart)
            {
                BindFromGlobalLifetimeScope();
            }
        }

        /// <summary>
        /// Finds the persistent GameLifetimeScope once at scene-composition time
        /// and binds this Gameplay scene to its DI container.
        /// </summary>
        public void BindFromGlobalLifetimeScope()
        {
            if (_isBound)
            {
                return;
            }

            GameLifetimeScope lifetimeScope =
                FindFirstObjectByType<GameLifetimeScope>();

            if (lifetimeScope == null)
            {
                throw new InvalidOperationException(
                    "GameplaySceneComposition could not find the persistent GameLifetimeScope. " +
                    "Start the game from ZombieWar_Boot or call Bind(IObjectResolver) explicitly.");
            }

            IObjectResolver resolver = lifetimeScope.Container;

            if (resolver == null)
            {
                throw new InvalidOperationException(
                    "GameLifetimeScope container has not been built yet.");
            }

            Bind(resolver);
        }

        /// <summary>
        /// Explicit binding entry point. This can be called by a scene loader later
        /// if the project moves away from automatic Start-based composition.
        /// </summary>
        public void Bind(
            IObjectResolver resolver)
        {
            if (_isBound)
            {
                return;
            }

            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            ValidateReferences();

            InitializeMap(resolver);
            InitializeLevel(resolver);
            InitializeScore(resolver);
            InitializeProjectile(resolver);
            InitializeZombie(resolver);
            InitializeBoss(resolver);
            InitializeCamera(resolver);
            InitializeSpawn(resolver);
            InitializeWeapon(resolver);
            InitializeSoldierAnimation(resolver);
            BindVFX(resolver);
            BindGameState(resolver);
            InitializeSoldierRuntime(resolver);

            // The GameState integration entry point may have started before this
            // scene existed. Reapply after scene gates and all runtime roots exist.
            resolver.Resolve<GameStateGameplayGateBridge>()
                .ReapplyCurrentState();

            _isBound = true;

            if (logSuccessfulBinding)
            {
                Debug.Log(
                    "[ZombieWar] GameplaySceneComposition bound successfully.",
                    this);
            }
        }

        private void InitializeMap(
            IObjectResolver resolver)
        {
            mapRuntimeRoot.Initialize(
                resolver.Resolve<IMapRuntime>(),
                resolver.Resolve<IMapRuntimeConfigurator>());
        }

        private void InitializeLevel(
            IObjectResolver resolver)
        {
            levelRuntimeRoot.Initialize(
                resolver.Resolve<ILevelRuntime>(),
                resolver.Resolve<ILevelRuntimeConfigurator>());
        }

        private void InitializeScore(
            IObjectResolver resolver)
        {
            scoreRuntimeRoot.Initialize(
                resolver.Resolve<IScoreRuntime>(),
                resolver.Resolve<IScoreRuntimeConfigurator>());
        }

        private void InitializeProjectile(
            IObjectResolver resolver)
        {
            projectileRuntimeRoot.Initialize(
                resolver.Resolve<IProjectileControllerFactory>(),
                resolver.Resolve<IProjectileLauncherFactory>());
        }

        private void InitializeZombie(
            IObjectResolver resolver)
        {
            zombieRuntimeRoot.Initialize(
                resolver.Resolve<IZombieFactory>(),
                resolver.Resolve<IHealthFactory>(),
                resolver.Resolve<ITargetRegistry>(),
                resolver.Resolve<IGameplayClock>(),
                resolver.Resolve<IZombieSoldierTargetRegistry>(),
                resolver.Resolve<IZombieAttackBinding>(),
                resolver.Resolve<IEventSubscriber>());
        }

        private void InitializeBoss(
            IObjectResolver resolver)
        {
            bossRuntimeRoot.Initialize(
                resolver.Resolve<IBossRuntime>(),
                resolver.Resolve<IBossRuntimeConfigurator>(),
                resolver.Resolve<IBossFactory>(),
                resolver.Resolve<IHealthFactory>(),
                resolver.Resolve<ITargetRegistry>(),
                resolver.Resolve<IGameplayClock>(),
                resolver.Resolve<IBossSoldierTargetRegistry>(),
                resolver.Resolve<IBossAttackBinding>(),
                resolver.Resolve<IEventSubscriber>());
        }

        private void InitializeCamera(
            IObjectResolver resolver)
        {
            cameraRuntimeRoot.Initialize(
                resolver.Resolve<ICameraRuntime>(),
                resolver.Resolve<ICameraRuntimeConfigurator>(),
                resolver.Resolve<ICameraBoundsProvider>());
        }

        private void InitializeSpawn(
            IObjectResolver resolver)
        {
            spawnRuntimeRoot.Initialize(
                resolver.Resolve<ISpawnRuntime>(),
                resolver.Resolve<ISpawnRuntimeConfigurator>(),
                resolver.Resolve<ISpawnRandom>(),
                resolver.Resolve<ISpawnSectorProvider>(),
                resolver.Resolve<ISpawnGameplayBoundsQuery>());
        }

        private void InitializeWeapon(
            IObjectResolver resolver)
        {
            weaponRuntimeRoot.Initialize(
                resolver.Resolve<IWeaponRuntime>(),
                resolver.Resolve<IWeaponAttackService>(),
                resolver.Resolve<IGameplayClock>(),
                resolver.Resolve<IWeaponProjectileBinding>(),
                resolver.Resolve<IWeaponMuzzleRegistry>(),
                resolver.Resolve<IEventSubscriber>());
        }


        private void InitializeSoldierAnimation(
            IObjectResolver resolver)
        {
            soldierAnimationRuntimeRoot.Initialize(
                resolver.Resolve<ISoldierWeaponAnimationRegistry>(),
                resolver.Resolve<IEventSubscriber>());
        }

        private void InitializeSoldierRuntime(
            IObjectResolver resolver)
        {
            soldierRuntimeRoot.Initialize(resolver);
        }

        private void BindVFX(
            IObjectResolver resolver)
        {
            if (vfxRuntimeRoot.IsBound)
            {
                return;
            }

            vfxRuntimeRoot.Bind(
                resolver.Resolve<IVFXRuntime>(),
                resolver.Resolve<IVFXRuntimeConfigurator>());
        }

        private void BindGameState(
            IObjectResolver resolver)
        {
            if (gameStateRuntimeRoot.IsBound)
            {
                return;
            }

            gameStateRuntimeRoot.Bind(
                resolver.Resolve<IGameStateRuntime>(),
                resolver.Resolve<IGameStateSceneGateBinding>());
        }

        private void ValidateReferences()
        {
            RequireReference(
                mapRuntimeRoot,
                nameof(mapRuntimeRoot));

            RequireReference(
                cameraRuntimeRoot,
                nameof(cameraRuntimeRoot));

            RequireReference(
                levelRuntimeRoot,
                nameof(levelRuntimeRoot));

            RequireReference(
                scoreRuntimeRoot,
                nameof(scoreRuntimeRoot));

            RequireReference(
                projectileRuntimeRoot,
                nameof(projectileRuntimeRoot));

            RequireReference(
                weaponRuntimeRoot,
                nameof(weaponRuntimeRoot));

            RequireReference(
                zombieRuntimeRoot,
                nameof(zombieRuntimeRoot));

            RequireReference(
                bossRuntimeRoot,
                nameof(bossRuntimeRoot));

            RequireReference(
                spawnRuntimeRoot,
                nameof(spawnRuntimeRoot));

            RequireReference(
                gameStateRuntimeRoot,
                nameof(gameStateRuntimeRoot));

            RequireReference(
                vfxRuntimeRoot,
                nameof(vfxRuntimeRoot));

            RequireReference(
                soldierAnimationRuntimeRoot,
                nameof(soldierAnimationRuntimeRoot));

            RequireReference(
                soldierRuntimeRoot,
                nameof(soldierRuntimeRoot));
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
                $"GameplaySceneComposition requires '{fieldName}' to be assigned.");
        }

        private void OnDestroy()
        {
            // Individual RuntimeRoot components own their own shutdown/unbind logic.
            // Do not double-dispose persistent DI singletons from this composition root.
            _isBound = false;
        }
    }
}
