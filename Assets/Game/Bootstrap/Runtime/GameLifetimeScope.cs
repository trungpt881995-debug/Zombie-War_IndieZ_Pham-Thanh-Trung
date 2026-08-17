using System.Collections.Generic;
using GeneralCore.AnalyticsDiagnostics;
using GeneralCore.Architecture;
using GeneralCore.UIInput;
using GameplayCore.Damage;
using GameplayCore.Entities;
using GameplayCore.Levels;
using GameplayCore.Random;
using GameplayCore.RuntimeState;
using GameplayCore.Session;
using GameplayCore.Time;
using GameplayCore.Targeting;
using ZombieWar.Features.Targeting.Domain;
using ZombieWar.Features.Targeting.Factories;
using ZombieWar.Features.Targeting.Registry;
using ZombieWar.Features.Targeting.Selection;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using ZombieWar.GameFlow.Commands;
using ZombieWar.GameFlow.Controller;
using ZombieWar.GameFlow.Domain;
using ZombieWar.GameFlow.Model;
using ZombieWar.GameFlow.StateMachine;
using ZombieWar.GameFlow.States;
using ZombieWar.GameFlow.View;
using ZombieWar.Features.Control.Input;
using ZombieWar.Features.Control.Ports;
using ZombieWar.Features.Damage.Controller;
using ZombieWar.Features.Damage.Model;
using ZombieWar.Features.Damage.View;
using ZombieWar.Features.Health.Factories;
using ZombieWar.Features.Soldier.Factories;
using ZombieWar.Features.Soldier.Movement;
using ZombieWar.Features.Soldier.Ports;
using ZombieWar.Features.Projectile.Ports;
using ZombieWar.Features.Weapon.Commands;
using ZombieWar.Features.Weapon.Factories;
using ZombieWar.Features.Weapon.Ports;
using ZombieWar.Features.Weapon.Services;
using ZombieWar.Features.Weapon.Strategies;
using ZombieWar.Features.Weapon.View;
using ZombieWar.Features.Zombie.Factories;
using ZombieWar.Features.Zombie.Ports;
using ZombieWar.Features.Map.Services;
using ZombieWar.Features.Camera.Commands;
using ZombieWar.Features.Camera.Ports;
using ZombieWar.Features.Camera.Services;
using ZombieWar.Features.Spawn.Commands;
using ZombieWar.Features.Spawn.Ports;
using ZombieWar.Features.Spawn.Services;
using ZombieWar.Features.Level.Commands;
using ZombieWar.Features.Level.Services;
using ZombieWar.Features.Boss.Commands;
using ZombieWar.Features.Boss.Factories;
using ZombieWar.Features.Boss.Ports;
using ZombieWar.Features.Boss.Services;
using ZombieWar.Features.Score.Commands;
using ZombieWar.Features.Score.Services;
using ZombieWar.Features.GameState.Commands;
using ZombieWar.Features.GameState.Controller;
using ZombieWar.Features.GameState.Model;
using ZombieWar.Features.GameState.Policies;
using ZombieWar.Features.GameState.Services;
using ZombieWar.Integration.GameState.GameFlow;
using ZombieWar.Integration.GameState.Level;
using ZombieWar.Integration.GameState.Runtime;
using ZombieWar.Integration.GameState.Soldier;
using ZombieWar.Integration.Boss;
using ZombieWar.Integration.Boss.Level;
using ZombieWar.Integration.Score.Zombie;
using ZombieWar.Integration.Score.Boss;
using ZombieWar.Integration.Score.Level;
using ZombieWar.Integration.Level.Zombie;
using ZombieWar.Integration.Level.Spawn;
using ZombieWar.Integration.Level.Soldier;
using ZombieWar.Integration.Spawn.Map;
using ZombieWar.Integration.Spawn.Runtime;
using ZombieWar.Integration.Zombie;
using ZombieWar.Integration.Camera.Map;
using ZombieWar.Integration.Weapon;
using ZombieWar.Integration.Soldier;
using ZombieWar.Integration.Soldier.Animation;
using ZombieWar.Integration.Soldier.Animation.Weapon;
using ZombieWar.Features.UI.Controller;
using ZombieWar.Features.UI.Model;
using ZombieWar.Features.UI.Ports;
using ZombieWar.Features.UI.Presentation;
using ZombieWar.Features.UI.Routing;
using ZombieWar.Features.UI.Services;
using ZombieWar.Integration.UI.GameFlow;
using ZombieWar.Integration.UI.GameState;
using ZombieWar.Integration.UI.Health;
using ZombieWar.Integration.UI.Level;
using ZombieWar.Integration.UI.Score;
using ZombieWar.Integration.UI.Weapon;
using ZombieWar.Features.VFX.Commands;
using ZombieWar.Features.VFX.Controller;
using ZombieWar.Features.VFX.Model;
using ZombieWar.Features.VFX.Services;
using ZombieWar.Integration.VFX.Weapon;
using ZombieWar.Integration.VFX.Projectile;
using ZombieWar.Integration.VFX.Zombie;
using ZombieWar.Integration.VFX.Boss;
using ZombieWar.Integration.VFX.Soldier;
using ZombieWar.Integration.VFX.GameState;
using ZombieWar.Features.Feedback.Commands;
using ZombieWar.Features.Feedback.Controller;
using ZombieWar.Features.Feedback.Model;
using ZombieWar.Features.Feedback.Policies;
using ZombieWar.Features.Feedback.Ports;
using ZombieWar.Features.Feedback.Services;
using ZombieWar.Integration.Feedback.Camera;
using ZombieWar.Integration.Feedback.Weapon;
using ZombieWar.Integration.Feedback.Boss;
using ZombieWar.Integration.Feedback.Soldier;
using ZombieWar.Integration.Feedback.Level;
using ZombieWar.Integration.Feedback.GameState;
using ZombieWar.Features.Audio.Commands;
using ZombieWar.Features.Audio.Controller;
using ZombieWar.Features.Audio.Model;
using ZombieWar.Features.Audio.Policies;
using ZombieWar.Features.Audio.Ports;
using ZombieWar.Features.Audio.Services;
using ZombieWar.Integration.Audio.Weapon;
using ZombieWar.Integration.Audio.Projectile;
using ZombieWar.Integration.Audio.Boss;
using ZombieWar.Integration.Audio.Zombie;
using ZombieWar.Integration.Audio.Soldier;
using ZombieWar.Integration.Audio.Level;
using ZombieWar.Integration.Audio.GameState;
using ZombieWar.Integration.Audio.GameFlow;
using ZombieWar.Integration.Audio.UI;
using ZombieWar.Infrastructure.Unity;

namespace ZombieWar.Bootstrap
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        protected override void Configure(IContainerBuilder builder)
        {
            var eventBus = new EventBus();
            builder.RegisterInstance(eventBus).As<IEventBus>().As<IEventSubscriber>();

            var commandBus = new CommandBus();
            builder.RegisterInstance(commandBus).As<ICommandBus>().As<ICommandRegistry>();

            var gameplayClock = new GameplayClock();
            builder.RegisterInstance(gameplayClock).AsSelf().As<IGameplayClock>().As<IGameplayClockControl>();

            builder.Register<IGameLogger, UnityGameLogger>(Lifetime.Singleton);
            builder.Register<IGameplaySession, GameplaySession>(Lifetime.Singleton);
            var entityIdGenerator = new SequentialEntityIdGenerator();
            builder.RegisterInstance(entityIdGenerator)
                .As<IEntityIdGenerator>();
            builder.Register<ILevelLifecycle, LevelLifecycle>(Lifetime.Singleton);
            var gameplayRandom =new XorShiftGameplayRandom(123456789);

            builder.RegisterInstance(gameplayRandom).AsSelf().As<IGameplayRandom>();
            builder.Register<IGameplayRuntimeState, GameplayRuntimeState>(Lifetime.Singleton);

            // Global gameplay-input gate. GameState/Pause can depend only on GeneralCore.UIInput.IInputGate.
            // ControlController observes the same instance via IGameplayInputState and immediately cancels active input when disabled.
            var inputGate = new GameplayInputGate(true);
            builder.RegisterInstance(inputGate).As<IInputGate>().As<IGameplayInputState>();

            // Shared Soldier movement-input buffer. Control writes through an Integration adapter;
            // Soldier Group reads the latest intent when its runtime is ticking.
            var soldierInputBuffer = new SoldierGroupInputBuffer();
            builder.RegisterInstance(soldierInputBuffer).As<ISoldierGroupInputBuffer>();

            builder.Register<ControlMovementIntentToSoldierAdapter>(Lifetime.Singleton)
                .As<IMovementIntentSink>();

            // Zombie War game-specific Damage implementation.
            builder.Register<DamageModel>(Lifetime.Singleton);
            builder.RegisterInstance(NullDamageView.Instance).As<IDamageView>();
            builder.Register<DamageController>(Lifetime.Singleton).As<IDamageService>();

            builder.Register<IHealthFactory, HealthFactory>(Lifetime.Singleton);

            // Normal Zombie AI/lifecycle core. Scene-specific pool, Unity motor/view and
            // Soldier Transform bindings are composed by ZombieRuntimeRoot.
            var zombieTargetProvider = new ZombieSoldierTargetProvider();
            builder.RegisterInstance(zombieTargetProvider)
                .As<IZombieTargetProvider>()
                .As<IZombieSoldierTargetRegistry>();
            builder.Register<ZombieAttackDamageAdapter>(Lifetime.Singleton)
                .As<IZombieAttackPort>()
                .As<IZombieAttackBinding>();
            builder.Register<ZombieVFXFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<ZombieAudioFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<CompositeZombiePresentationFeedbackPort>(Lifetime.Singleton)
                .As<IZombieFeedbackPort>();
            builder.Register<IZombieFactory, ZombieFactory>(Lifetime.Singleton);

            // Map runtime owns current-map lifecycle and immutable spatial context.
            // Scene-specific MapCatalogConfig/MapAssetLoaderBehaviour are bound by MapRuntimeRoot.
            builder.Register<MapRuntime>(Lifetime.Singleton)
                .As<IMapRuntime>()
                .As<IMapRuntimeConfigurator>();

            // Camera runtime is feature-isolated. Map bounds are adapted through a pure
            // cross-feature provider; Soldier target + concrete Camera rig remain scene-specific.
            builder.Register<MapCameraBoundsProvider>(Lifetime.Singleton)
                .As<ICameraBoundsProvider>();
            builder.Register<CameraRuntime>(Lifetime.Singleton)
                .As<ICameraRuntime>()
                .As<ICameraRuntimeConfigurator>();
            builder.Register<RequestCameraShakeCommandHandler>(Lifetime.Singleton);

            // Spawn runtime orchestrates cadence/placement/capacity. Map geometry and GameplayRandom
            // are adapted through DI; Camera visibility, NavMesh and ZombieRuntime are scene-specific.
            builder.Register<MapSpawnSectorProvider>(Lifetime.Singleton).As<ISpawnSectorProvider>();
            builder.Register<MapGameplayBoundsQuery>(Lifetime.Singleton).As<ISpawnGameplayBoundsQuery>();
            builder.Register<GameplayRandomSpawnAdapter>(Lifetime.Singleton).As<ISpawnRandom>();
            builder.Register<SpawnRuntime>(Lifetime.Singleton).As<ISpawnRuntime>().As<ISpawnRuntimeConfigurator>();
            builder.Register<StartZombieSpawningCommandHandler>(Lifetime.Singleton);
            builder.Register<StopZombieSpawningCommandHandler>(Lifetime.Singleton);
            builder.Register<SetSpawnDifficultyCommandHandler>(Lifetime.Singleton);


            // Level runtime is the single source of truth for Game Level progression,
            // Soldier Group Level, Normal Zombie Kill Count and Boss Phase. Execution remains
            // in Soldier/Spawn/Boss/GameFlow integrations.
            builder.Register<LevelRuntime>(Lifetime.Singleton)
                .As<ILevelRuntime>()
                .As<ILevelRuntimeConfigurator>();
            builder.Register<BeginGameLevelCommandHandler>(Lifetime.Singleton);
            builder.Register<RegisterNormalZombieKillCommandHandler>(Lifetime.Singleton);
            builder.Register<RegisterBossDefeatedCommandHandler>(Lifetime.Singleton);
            builder.Register<SetLevelProgressionEnabledCommandHandler>(Lifetime.Singleton);

            // Boss runtime owns Boss A/B/C lifecycle/AI/combat orchestration. Scene-specific pools,
            // BossCatalogConfig, Unity views/motors and Soldier Transform binding are composed by BossRuntimeRoot.
            var bossTargetProvider = new BossSoldierTargetProvider();
            builder.RegisterInstance(bossTargetProvider)
                .As<IBossTargetProvider>()
                .As<IBossSoldierTargetRegistry>();
            builder.Register<BossAttackDamageAdapter>(Lifetime.Singleton)
                .As<IBossAttackPort>()
                .As<IBossAttackBinding>();
            builder.Register<BossVFXFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<BossGameFeelFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<CompositeBossFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<BossAudioFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<CompositeBossPresentationFeedbackPort>(Lifetime.Singleton)
                .As<IBossFeedbackPort>();
            builder.Register<IBossFactory, BossFactory>(Lifetime.Singleton);
            builder.Register<BossRuntime>(Lifetime.Singleton)
                .As<IBossRuntime>()
                .As<IBossRuntimeConfigurator>();
            builder.Register<SpawnLevelBossesCommandHandler>(Lifetime.Singleton);
            builder.Register<SetBossGameplayEnabledCommandHandler>(Lifetime.Singleton);
            builder.Register<CancelAllBossesCommandHandler>(Lifetime.Singleton);
            builder.Register<LevelBossPhaseBridge>(Lifetime.Singleton);
            builder.Register<BossDeathToLevelBridge>(Lifetime.Singleton);

            // Score runtime owns run score, current-level score and replay checkpoint.
            // Scene-specific ScoreConfig is bound by ScoreRuntimeRoot; Zombie/Boss/Level facts
            // cross the feature boundary only through integration bridges.
            builder.Register<ScoreRuntime>(Lifetime.Singleton)
                .As<IScoreRuntime>()
                .As<IScoreRuntimeConfigurator>();
            builder.Register<StartScoreRunCommandHandler>(Lifetime.Singleton);
            builder.Register<BeginScoreLevelCommandHandler>(Lifetime.Singleton);
            builder.Register<ReplayScoreLevelCommandHandler>(Lifetime.Singleton);
            builder.Register<AwardScoreCommandHandler>(Lifetime.Singleton);
            builder.Register<SetScoringEnabledCommandHandler>(Lifetime.Singleton);
            builder.Register<ZombieScoreBridge>(Lifetime.Singleton);
            builder.Register<BossScoreBridge>(Lifetime.Singleton);
            builder.Register<LevelScoreLifecycleBridge>(Lifetime.Singleton);

            // GameState is the single source of truth for runtime gameplay state.
            // GameFlow keeps ownership of Boot/MainMenu/Loading/navigation; bridges synchronize
            // runtime gates and terminal results without introducing reverse feature dependencies.
            builder.Register<GameStateModel>(Lifetime.Singleton);
            builder.Register<GameplayStateTransitionPolicy>(Lifetime.Singleton)
                .As<IGameplayStateTransitionPolicy>();
            builder.Register<GameStateController>(Lifetime.Singleton);
            builder.Register<GameStateRuntime>(Lifetime.Singleton)
                .As<IGameStateRuntime>()
                .As<IGameStateRuntimeConfigurator>();
            builder.Register<BeginGameplayCommandHandler>(Lifetime.Singleton);
            builder.Register<PauseGameplayCommandHandler>(Lifetime.Singleton);
            builder.Register<ResumeGameplayCommandHandler>(Lifetime.Singleton);
            builder.Register<EnterGameOverCommandHandler>(Lifetime.Singleton);
            builder.Register<EnterLevelCompleteCommandHandler>(Lifetime.Singleton);
            builder.Register<EnterEndGameCommandHandler>(Lifetime.Singleton);
            builder.Register<DeactivateGameplayCommandHandler>(Lifetime.Singleton);

            var gameStateSceneGates = new GameStateSceneGateRegistry();
            builder.RegisterInstance(gameStateSceneGates)
                .As<IGameStateSceneGateBinding>()
                .As<IGameStateSceneGateRegistry>();

            builder.Register<GameStateSoldierBinding>(Lifetime.Singleton)
                .As<IGameStateSoldierBinding>()
                .As<IGameStateSoldierGate>();
            builder.Register<GameStateGameplayGateBridge>(Lifetime.Singleton);
            builder.Register<SoldierDefeatGameStateBridge>(Lifetime.Singleton);
            builder.Register<LevelGameStateBridge>(Lifetime.Singleton);
            builder.Register<GameFlowGameStateBridge>(Lifetime.Singleton);

            builder.Register<ZombieKillToLevelProgressAdapter>(Lifetime.Singleton);
            builder.Register<LevelSpawnProgressionBridge>(Lifetime.Singleton);
            builder.Register<LevelSoldierProgressionBridge>(Lifetime.Singleton)
                .AsSelf()
                .As<ILevelSoldierBinding>();

            // UI is presentation-only. GameFlow owns screen/application state; GameState owns
            // runtime gameplay state. Scene views are bound later through UIRuntimeRoot.
            builder.Register<UIScreenModel>(Lifetime.Singleton);
            builder.Register<GameplayHudModel>(Lifetime.Singleton);
            builder.Register<UIScreenRouter>(Lifetime.Singleton).As<IUIScreenRouter>();
            builder.Register<GameFlowUIActionPort>(Lifetime.Singleton)
                .As<IUIFlowActionPort>()
                .As<IGameFlowUIActionContext>();
            builder.Register<GameStateUIPausePort>(Lifetime.Singleton).As<IGameplayPausePort>();
            builder.Register<WeaponUISelectionPort>(Lifetime.Singleton).As<IWeaponSelectionPort>();

            builder.Register<MainMenuController>(Lifetime.Singleton);
            builder.Register<GameplayHudController>(Lifetime.Singleton);
            builder.Register<PauseController>(Lifetime.Singleton);
            builder.Register<LevelCompleteController>(Lifetime.Singleton);
            builder.Register<GameOverController>(Lifetime.Singleton);
            builder.Register<EndGameController>(Lifetime.Singleton);

            builder.Register<ScorePresenter>(Lifetime.Singleton);
            builder.Register<HealthPresenter>(Lifetime.Singleton);
            builder.Register<LevelPresenter>(Lifetime.Singleton);
            builder.Register<WeaponHudPresenter>(Lifetime.Singleton);
            builder.Register<UIRuntime>(Lifetime.Singleton)
                .AsSelf()
                .As<IUIRuntime>();

            builder.Register<GameFlowUIBridge>(Lifetime.Singleton);
            builder.Register<ScoreUIBridge>(Lifetime.Singleton);
            builder.Register<LevelUIBridge>(Lifetime.Singleton);
            builder.Register<HealthUIBridge>(Lifetime.Singleton).As<IUIHealthBinding>();
            builder.Register<WeaponUIBridge>(Lifetime.Singleton);

            // VFX runtime is presentation-only. Scene-specific catalog/pools are bound by VFXRuntimeRoot.
            builder.Register<VFXModel>(Lifetime.Singleton);
            builder.Register<VFXController>(Lifetime.Singleton);
            builder.Register<VFXRuntime>(Lifetime.Singleton).As<IVFXRuntime>().As<IVFXRuntimeConfigurator>();
            builder.Register<PlayVFXCommandHandler>(Lifetime.Singleton);
            builder.Register<StopVFXCommandHandler>(Lifetime.Singleton);
            builder.Register<SetVFXModeCommandHandler>(Lifetime.Singleton);
            builder.Register<CancelAllVFXCommandHandler>(Lifetime.Singleton);
            builder.Register<SoldierDamageVFXBridge>(Lifetime.Singleton).AsSelf().As<ISoldierVFXAnchorBinding>();
            builder.Register<GameStateVFXBridge>(Lifetime.Singleton);

            // Feedback runtime orchestrates game feel only: Camera/Haptic/Screen/Recoil.
            // VFX remains independent; single feedback-port contracts are composed below
            // so VFX and game-feel both receive each source fact exactly once.
            builder.Register<FeedbackModel>(Lifetime.Singleton);

            builder.Register<FeedbackPreferences>(Lifetime.Singleton)
                .AsSelf()
                .As<IFeedbackPreferences>();

            builder.Register<HapticCooldownPolicy>(Lifetime.Singleton)
                .As<IHapticCooldownPolicy>();

            builder.Register<FeedbackPriorityPolicy>(Lifetime.Singleton)
                .As<IFeedbackPriorityPolicy>();

            builder.Register<FeedbackController>(Lifetime.Singleton);

            builder.Register<FeedbackRuntime>(Lifetime.Singleton)
                .As<IFeedbackRuntime>()
                .As<IFeedbackRuntimeConfigurator>();

            builder.Register<CameraFeedbackAdapter>(Lifetime.Singleton)
                .As<ICameraFeedbackPort>();

            builder.RegisterInstance(NullRecoilFeedbackPort.Instance)
                .As<IRecoilFeedbackPort>();

            builder.Register<PlayFeedbackCommandHandler>(Lifetime.Singleton);
            builder.Register<SetFeedbackModeCommandHandler>(Lifetime.Singleton);
            builder.Register<CancelFeedbackCommandHandler>(Lifetime.Singleton);

            builder.Register<SoldierDamageFeedbackBridge>(Lifetime.Singleton)
                .AsSelf()
                .As<IFeedbackSoldierBinding>();

            builder.Register<LevelFeedbackBridge>(Lifetime.Singleton);
            builder.Register<GameStateFeedbackBridge>(Lifetime.Singleton);

            // Audio owns sound playback/mixing policy only. World SFX follows GameState,
            // while global UI/Music remain available through category-specific policy.
            builder.Register<AudioModel>(Lifetime.Singleton);

            builder.Register<AudioPreferences>(Lifetime.Singleton)
                .AsSelf()
                .As<IAudioPreferences>();

            builder.Register<AudioConcurrencyPolicy>(Lifetime.Singleton)
                .As<IAudioConcurrencyPolicy>();

            builder.Register<AudioModePolicy>(Lifetime.Singleton)
                .As<IAudioModePolicy>();

            builder.Register<SystemAudioRandom>(Lifetime.Singleton)
                .As<IAudioRandom>();

            builder.Register<AudioController>(Lifetime.Singleton);
            builder.Register<MusicController>(Lifetime.Singleton);

            builder.Register<AudioRuntime>(Lifetime.Singleton)
                .As<IAudioRuntime>()
                .As<IAudioRuntimeConfigurator>()
                .As<IAudioRuntimeDriver>();

            builder.Register<PlayAudioCommandHandler>(Lifetime.Singleton);
            builder.Register<StopAudioCommandHandler>(Lifetime.Singleton);
            builder.Register<SetWorldAudioModeCommandHandler>(Lifetime.Singleton);
            builder.Register<PlayMusicCommandHandler>(Lifetime.Singleton);
            builder.Register<StopMusicCommandHandler>(Lifetime.Singleton);
            builder.Register<CancelAllAudioCommandHandler>(Lifetime.Singleton);

            builder.Register<SoldierDamageAudioBridge>(Lifetime.Singleton)
                .AsSelf()
                .As<IAudioSoldierBinding>();

            builder.Register<LevelAudioBridge>(Lifetime.Singleton);
            builder.Register<GameStateAudioBridge>(Lifetime.Singleton);
            builder.Register<GameFlowMusicBridge>(Lifetime.Singleton);

            builder.Register<UIAudioPort>(Lifetime.Singleton)
                .As<IUIAudioPort>();

            // Shared Targeting services. TargetingController itself is intentionally NOT
            // registered as a singleton: ITargetingFactory creates one independent session per Soldier.
            builder.Register<ITargetRegistry, TargetRegistry>(Lifetime.Singleton);
            builder.Register<IDistanceMetric, PlanarXZDistanceMetric>(Lifetime.Singleton);
            builder.Register<NearestTargetSelector>(Lifetime.Singleton)
                .As<ITargetSelector<TargetingContext, ITargetCandidate>>();
            builder.Register<ITargetValidator, TargetValidator>(Lifetime.Singleton);
            builder.Register<ITargetingFactory, TargetingFactory>(Lifetime.Singleton);

            // Soldier runtime remains feature-isolated. Cross-feature Targeting and Control
            // translations live in ZombieWar.Integration.Soldier.
            builder.Register<TargetingToSoldierAdapter>(Lifetime.Singleton)
                .As<ISoldierTargetingPort>();

            // Weapon core is shared by the whole Soldier Group, while WeaponAttackService
            // owns independent per-Soldier fire sessions. Runtime config/projectile binding
            // is completed by WeaponRuntimeRoot in the Gameplay Scene.
            builder.RegisterInstance(NullWeaponView.Instance).As<IWeaponView>();
            builder.Register<WeaponFlameDamagePort>(Lifetime.Singleton).As<IWeaponFlamePort>();
            builder.Register<WeaponVFXFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<WeaponGameFeelFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<CompositeWeaponFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<WeaponAudioFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<CompositeWeaponPresentationFeedbackPort>(Lifetime.Singleton).AsSelf();

            builder.Register<SoldierWeaponAnimationRegistry>(Lifetime.Singleton)
                .AsSelf()
                .As<ISoldierWeaponAnimationRegistry>();

            builder.Register<WeaponSoldierAnimationFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<CompositeWeaponAnimationPresentationFeedbackPort>(Lifetime.Singleton)
                .As<IWeaponFeedbackPort>();

            builder.Register<WeaponRuntime>(Lifetime.Singleton).As<IWeaponRuntime>();

            var weaponMuzzleRegistry = new WeaponMuzzleRegistry();
            builder.RegisterInstance(weaponMuzzleRegistry)
                .As<IWeaponMuzzleProvider>()
                .As<IWeaponMuzzleRegistry>();

            var weaponProjectileAdapter = new WeaponToProjectileAdapter();
            builder.RegisterInstance(weaponProjectileAdapter)
                .As<IWeaponProjectilePort>()
                .As<IWeaponProjectileBinding>();

            builder.Register<IWeaponFireStrategyProvider, WeaponFireStrategyProvider>(Lifetime.Singleton);
            builder.Register<IWeaponFireSessionFactory, WeaponFireSessionFactory>(Lifetime.Singleton);
            builder.Register<IWeaponAttackService, WeaponAttackService>(Lifetime.Singleton);
            builder.Register<SelectWeaponCommandHandler>(Lifetime.Singleton);

            builder.Register<WeaponToSoldierAttackAdapter>(Lifetime.Singleton)
                .As<ISoldierAttackPort>();
            builder.Register<WeaponTargetRangeProvider>(Lifetime.Singleton)
                .As<ITargetRangeProvider>();

            builder.Register<ISoldierMovementSolver, SoldierMovementSolver>(
                Lifetime.Singleton);

            builder.Register<ISoldierFactory, SoldierFactory>(
                Lifetime.Singleton);

            builder.Register<ISoldierGroupFactory, SoldierGroupFactory>(
                Lifetime.Singleton);

            // Projectile gameplay is hitscan-only. Physical projectile factories, pools,
            // impact policies and flight simulation are intentionally not registered.
            // Presentation fan-out is composed here at the application boundary.
            builder.Register<ProjectileVFXFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<ProjectileAudioFeedbackPort>(Lifetime.Singleton).AsSelf();
            builder.Register<ProjectilePresentationFeedbackPort>(Lifetime.Singleton)
                .As<IProjectileFeedbackPort>();

            builder.Register<GameFlowModel>(Lifetime.Singleton);
            builder.RegisterInstance(new NullGameFlowView()).As<IGameFlowView>();

            var states = new List<IGameFlowState>
            {
                new BootState(),
                new MainMenuState(),
                new LoadingState(),
                new GameplayState(),
                new PausedState(),
                new LevelCompleteState(),
                new GameOverState(),
                new EndGameState()
            };
            builder.RegisterInstance(states).As<IReadOnlyList<IGameFlowState>>();

            builder.Register<GameFlowStateMachine>(Lifetime.Singleton);
            builder.Register<GameFlowController>(Lifetime.Singleton);
            builder.Register<ChangeGameFlowStateCommandHandler>(Lifetime.Singleton);

            builder.RegisterEntryPoint<UnityGameplayClockDriver>();
            builder.RegisterEntryPoint<WeaponCommandRegistration>();
            builder.RegisterEntryPoint<CameraCommandRegistration>();
            builder.RegisterEntryPoint<SpawnCommandRegistration>();
            builder.RegisterEntryPoint<LevelCommandRegistration>();
            builder.RegisterEntryPoint<LevelIntegrationRegistration>();
            builder.RegisterEntryPoint<BossCommandRegistration>();
            builder.RegisterEntryPoint<BossIntegrationRegistration>();
            builder.RegisterEntryPoint<ScoreCommandRegistration>();
            builder.RegisterEntryPoint<ScoreIntegrationRegistration>();
            builder.RegisterEntryPoint<GameStateBootstrapRegistration>();
            builder.RegisterEntryPoint<GameStateCommandRegistration>();
            builder.RegisterEntryPoint<GameBootstrap>();
            builder.RegisterEntryPoint<SpawnGameFlowLifecycleRegistration>();
            builder.RegisterEntryPoint<GameStateIntegrationRegistration>();
            builder.RegisterEntryPoint<UIIntegrationRegistration>();
            builder.RegisterEntryPoint<VFXCommandRegistration>();
            builder.RegisterEntryPoint<VFXIntegrationRegistration>();
            builder.RegisterEntryPoint<FeedbackCommandRegistration>();
            builder.RegisterEntryPoint<FeedbackIntegrationRegistration>();
            builder.RegisterEntryPoint<AudioCommandRegistration>();
            builder.RegisterEntryPoint<AudioIntegrationRegistration>();
        }
    }
}
