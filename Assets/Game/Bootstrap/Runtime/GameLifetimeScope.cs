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
using ZombieWar.Features.Projectile.Factories;
using ZombieWar.Features.Projectile.Impact;
using ZombieWar.Features.Projectile.Ports;
using ZombieWar.Features.Weapon.Commands;
using ZombieWar.Features.Weapon.Factories;
using ZombieWar.Features.Weapon.Ports;
using ZombieWar.Features.Weapon.Services;
using ZombieWar.Features.Weapon.Strategies;
using ZombieWar.Features.Weapon.View;
using ZombieWar.Features.Zombie.Factories;
using ZombieWar.Features.Zombie.Ports;
using ZombieWar.Integration.Zombie;
using ZombieWar.Integration.Weapon;
using ZombieWar.Integration.Soldier;
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
            builder.Register<IEntityIdGenerator, SequentialEntityIdGenerator>(Lifetime.Singleton);
            builder.Register<ILevelLifecycle, LevelLifecycle>(Lifetime.Singleton);
            builder.Register<IGameplayRandom, XorShiftGameplayRandom>(Lifetime.Singleton);
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
            builder.RegisterInstance(NullZombieFeedbackPort.Instance).As<IZombieFeedbackPort>();
            builder.Register<IZombieFactory, ZombieFactory>(Lifetime.Singleton);


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
            builder.RegisterInstance(NullWeaponFlamePort.Instance).As<IWeaponFlamePort>();
            builder.RegisterInstance(NullWeaponFeedbackPort.Instance).As<IWeaponFeedbackPort>();
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

            // Projectile core remains feature-isolated. Scene-specific ProjectilePool/Driver
            // are composed by ProjectileRuntimeRoot; shared factories/policies live in DI.
            builder.Register<IProjectileImpactPolicyProvider, ProjectileImpactPolicyProvider>(
                Lifetime.Singleton);
            builder.RegisterInstance(NullProjectileExplosionPort.Instance)
                .As<IProjectileExplosionPort>();
            builder.RegisterInstance(NullProjectileFeedbackPort.Instance)
                .As<IProjectileFeedbackPort>();
            builder.Register<IProjectileControllerFactory, ProjectileControllerFactory>(
                Lifetime.Singleton);
            builder.Register<IProjectileLauncherFactory, ProjectileLauncherFactory>(
                Lifetime.Singleton);

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
            builder.RegisterEntryPoint<GameBootstrap>();
        }
    }
}
