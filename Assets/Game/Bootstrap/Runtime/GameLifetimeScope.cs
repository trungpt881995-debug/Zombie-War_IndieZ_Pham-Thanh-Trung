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

            // Temporary output until Soldier Feature provides the real MovementIntent -> CommandBus adapter.
            builder.RegisterInstance(NullMovementIntentSink.Instance).As<IMovementIntentSink>();

            // Zombie War game-specific Damage implementation.
            builder.Register<DamageModel>(Lifetime.Singleton);
            builder.RegisterInstance(NullDamageView.Instance).As<IDamageView>();
            builder.Register<DamageController>(Lifetime.Singleton).As<IDamageService>();

            builder.Register<IHealthFactory, HealthFactory>(Lifetime.Singleton);

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
            builder.RegisterEntryPoint<GameBootstrap>();
        }
    }
}
