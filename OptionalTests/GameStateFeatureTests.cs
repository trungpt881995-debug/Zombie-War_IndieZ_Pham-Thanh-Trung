using GeneralCore.Architecture;
using NUnit.Framework;
using ZombieWar.Features.GameState.Controller;
using ZombieWar.Features.GameState.Domain;
using ZombieWar.Features.GameState.Events;
using ZombieWar.Features.GameState.Model;
using ZombieWar.Features.GameState.Policies;
using ZombieWar.Features.GameState.Services;

namespace ZombieWar.Features.GameState.Tests
{
    public sealed class GameStateFeatureTests
    {
        private static GameStateRuntime Runtime(EventBus bus = null, bool initialize = true)
        {
            bus ??= new EventBus();
            var model = new GameStateModel();
            var controller = new GameStateController(model, new GameplayStateTransitionPolicy(), bus);
            var runtime = new GameStateRuntime(model, controller);
            if (initialize) runtime.Initialize();
            return runtime;
        }

        [Test] public void Runtime_Starts_NotInitialized_WhenNotConfigured(){var r=Runtime(initialize:false);Assert.IsFalse(r.IsInitialized);Assert.AreEqual(GameplayStateId.Inactive,r.State);}
        [Test] public void Initialize_SetsInactive(){var r=Runtime();Assert.IsTrue(r.IsInitialized);Assert.AreEqual(GameplayStateId.Inactive,r.State);Assert.AreEqual(0,r.Snapshot.TransitionSequence);}
        [Test] public void Initialize_IsIdempotent(){var r=Runtime();r.Initialize();Assert.AreEqual(GameplayStateId.Inactive,r.State);Assert.AreEqual(0,r.Snapshot.TransitionSequence);}
        [Test] public void Shutdown_ResetsAndDisables(){var r=Runtime();r.BeginGameplay();r.Shutdown();Assert.IsFalse(r.IsInitialized);Assert.AreEqual(GameplayStateId.Inactive,r.State);Assert.AreEqual(0,r.Snapshot.TransitionSequence);}
        [Test] public void TransitionBeforeInitialize_Rejected(){var r=Runtime(initialize:false);var x=r.BeginGameplay();Assert.IsFalse(x.Accepted);Assert.AreEqual(GameplayStateTransitionFailure.NotInitialized,x.Failure);}
        [Test] public void BeginGameplay_Accepted(){var r=Runtime();var x=r.BeginGameplay();Assert.IsTrue(x.Accepted);Assert.AreEqual(GameplayStateId.Playing,r.State);Assert.AreEqual(GameplayStateTransitionReason.GameFlowGameplayReady,x.Reason);}
        [Test] public void BeginGameplay_Twice_SecondRejected(){var r=Runtime();r.BeginGameplay();var x=r.BeginGameplay();Assert.IsFalse(x.Accepted);Assert.AreEqual(GameplayStateTransitionFailure.SameState,x.Failure);Assert.AreEqual(1,r.Snapshot.TransitionSequence);}
        [Test] public void Pause_FromPlaying(){var r=Runtime();r.BeginGameplay();var x=r.Pause();Assert.IsTrue(x.Accepted);Assert.AreEqual(GameplayStateId.Paused,r.State);Assert.AreEqual(GameplayStateTransitionReason.UserPauseRequested,x.Reason);}
        [Test] public void Resume_FromPaused(){var r=Runtime();r.BeginGameplay();r.Pause();var x=r.Resume();Assert.IsTrue(x.Accepted);Assert.AreEqual(GameplayStateId.Playing,r.State);Assert.AreEqual(GameplayStateTransitionReason.UserResumeRequested,x.Reason);}
        [Test] public void Pause_FromInactive_Rejected(){var r=Runtime();Assert.IsFalse(r.Pause().Accepted);}
        [Test] public void Resume_FromInactive_Rejected(){var r=Runtime();Assert.IsFalse(r.Resume().Accepted);}
        [Test] public void BeginGameplay_FromPaused_Rejected(){var r=Runtime();r.BeginGameplay();r.Pause();var x=r.BeginGameplay();Assert.IsFalse(x.Accepted);Assert.AreEqual(GameplayStateTransitionFailure.InvalidTransition,x.Failure);Assert.AreEqual(GameplayStateId.Paused,r.State);}
        [Test] public void GameOver_FromPlaying(){var r=Runtime();r.BeginGameplay();Assert.IsTrue(r.EnterGameOver().Accepted);Assert.AreEqual(GameplayStateId.GameOver,r.State);}
        [Test] public void GameOver_FromPaused(){var r=Runtime();r.BeginGameplay();r.Pause();Assert.IsTrue(r.EnterGameOver().Accepted);Assert.AreEqual(GameplayStateId.GameOver,r.State);}
        [Test] public void LevelComplete_FromPlaying(){var r=Runtime();r.BeginGameplay();Assert.IsTrue(r.EnterLevelComplete().Accepted);Assert.AreEqual(GameplayStateId.LevelComplete,r.State);}
        [Test] public void LevelComplete_FromPaused(){var r=Runtime();r.BeginGameplay();r.Pause();Assert.IsTrue(r.EnterLevelComplete().Accepted);Assert.AreEqual(GameplayStateId.LevelComplete,r.State);}
        [Test] public void EndGame_FromPlaying(){var r=Runtime();r.BeginGameplay();Assert.IsTrue(r.EnterEndGame().Accepted);Assert.AreEqual(GameplayStateId.EndGame,r.State);}
        [Test] public void EndGame_FromPaused(){var r=Runtime();r.BeginGameplay();r.Pause();Assert.IsTrue(r.EnterEndGame().Accepted);Assert.AreEqual(GameplayStateId.EndGame,r.State);}
        [Test] public void GameOver_CannotReturnDirectlyToPlaying(){var r=Runtime();r.BeginGameplay();r.EnterGameOver();Assert.IsFalse(r.BeginGameplay().Accepted);Assert.AreEqual(GameplayStateId.GameOver,r.State);}
        [Test] public void LevelComplete_CannotReturnDirectlyToPlaying(){var r=Runtime();r.BeginGameplay();r.EnterLevelComplete();Assert.IsFalse(r.BeginGameplay().Accepted);}
        [Test] public void EndGame_CannotReturnDirectlyToPlaying(){var r=Runtime();r.BeginGameplay();r.EnterEndGame();Assert.IsFalse(r.BeginGameplay().Accepted);}
        [Test] public void Playing_CanDeactivate(){var r=Runtime();r.BeginGameplay();Assert.IsTrue(r.Deactivate().Accepted);Assert.AreEqual(GameplayStateId.Inactive,r.State);}
        [Test] public void Paused_CanDeactivate(){var r=Runtime();r.BeginGameplay();r.Pause();Assert.IsTrue(r.Deactivate().Accepted);Assert.AreEqual(GameplayStateId.Inactive,r.State);}
        [Test] public void GameOver_CanDeactivate(){var r=Runtime();r.BeginGameplay();r.EnterGameOver();Assert.IsTrue(r.Deactivate().Accepted);}
        [Test] public void LevelComplete_CanDeactivate(){var r=Runtime();r.BeginGameplay();r.EnterLevelComplete();Assert.IsTrue(r.Deactivate().Accepted);}
        [Test] public void EndGame_CanDeactivate(){var r=Runtime();r.BeginGameplay();r.EnterEndGame();Assert.IsTrue(r.Deactivate().Accepted);}
        [Test] public void Deactivate_AlreadyInactive_Rejected(){var r=Runtime();var x=r.Deactivate();Assert.IsFalse(x.Accepted);Assert.AreEqual(GameplayStateTransitionFailure.SameState,x.Failure);}
        [Test] public void Snapshot_TracksPrevious(){var r=Runtime();r.BeginGameplay();r.Pause();Assert.AreEqual(GameplayStateId.Playing,r.Snapshot.Previous);Assert.AreEqual(GameplayStateId.Paused,r.Snapshot.Current);}
        [Test] public void Sequence_IncrementsOnlyAccepted(){var r=Runtime();r.BeginGameplay();r.BeginGameplay();r.Pause();r.Pause();Assert.AreEqual(2,r.Snapshot.TransitionSequence);}
        [Test] public void AcceptedTransition_PublishesExactlyOneEvent(){var b=new EventBus();int n=0;b.Subscribe<GameplayStateChangedEvent>(_=>n++);var r=Runtime(b);r.BeginGameplay();Assert.AreEqual(1,n);}
        [Test] public void RejectedTransition_PublishesNoEvent(){var b=new EventBus();int n=0;b.Subscribe<GameplayStateChangedEvent>(_=>n++);var r=Runtime(b);r.Pause();Assert.AreEqual(0,n);}
        [Test] public void Event_CarriesSequence(){var b=new EventBus();long seq=0;b.Subscribe<GameplayStateChangedEvent>(e=>seq=e.Sequence);var r=Runtime(b);r.BeginGameplay();r.Pause();Assert.AreEqual(2,seq);}
        [Test] public void Event_CarriesPreviousAndCurrent(){var b=new EventBus();GameplayStateChangedEvent last=default;b.Subscribe<GameplayStateChangedEvent>(e=>last=e);var r=Runtime(b);r.BeginGameplay();r.Pause();Assert.AreEqual(GameplayStateId.Playing,last.Previous);Assert.AreEqual(GameplayStateId.Paused,last.Current);}
        [Test] public void GameOverReason_IsSoldierDefeated(){var r=Runtime();r.BeginGameplay();Assert.AreEqual(GameplayStateTransitionReason.SoldierGroupDefeated,r.EnterGameOver().Reason);}
        [Test] public void LevelCompleteReason_IsCorrect(){var r=Runtime();r.BeginGameplay();Assert.AreEqual(GameplayStateTransitionReason.GameLevelCompleted,r.EnterLevelComplete().Reason);}
        [Test] public void EndGameReason_IsCorrect(){var r=Runtime();r.BeginGameplay();Assert.AreEqual(GameplayStateTransitionReason.GameCompleted,r.EnterEndGame().Reason);}
        [Test] public void DeactivateReason_IsCorrect(){var r=Runtime();r.BeginGameplay();Assert.AreEqual(GameplayStateTransitionReason.GameFlowDeactivated,r.Deactivate().Reason);}

        [TestCase(GameplayStateId.Inactive,GameplayStateId.Playing,true)]
        [TestCase(GameplayStateId.Inactive,GameplayStateId.Paused,false)]
        [TestCase(GameplayStateId.Inactive,GameplayStateId.GameOver,false)]
        [TestCase(GameplayStateId.Inactive,GameplayStateId.LevelComplete,false)]
        [TestCase(GameplayStateId.Inactive,GameplayStateId.EndGame,false)]
        [TestCase(GameplayStateId.Playing,GameplayStateId.Paused,true)]
        [TestCase(GameplayStateId.Playing,GameplayStateId.GameOver,true)]
        [TestCase(GameplayStateId.Playing,GameplayStateId.LevelComplete,true)]
        [TestCase(GameplayStateId.Playing,GameplayStateId.EndGame,true)]
        [TestCase(GameplayStateId.Playing,GameplayStateId.Inactive,true)]
        [TestCase(GameplayStateId.Paused,GameplayStateId.Playing,true)]
        [TestCase(GameplayStateId.Paused,GameplayStateId.GameOver,true)]
        [TestCase(GameplayStateId.Paused,GameplayStateId.LevelComplete,true)]
        [TestCase(GameplayStateId.Paused,GameplayStateId.EndGame,true)]
        [TestCase(GameplayStateId.Paused,GameplayStateId.Inactive,true)]
        [TestCase(GameplayStateId.GameOver,GameplayStateId.Inactive,true)]
        [TestCase(GameplayStateId.LevelComplete,GameplayStateId.Inactive,true)]
        [TestCase(GameplayStateId.EndGame,GameplayStateId.Inactive,true)]
        [TestCase(GameplayStateId.GameOver,GameplayStateId.Playing,false)]
        [TestCase(GameplayStateId.LevelComplete,GameplayStateId.Playing,false)]
        [TestCase(GameplayStateId.EndGame,GameplayStateId.Playing,false)]
        [TestCase(GameplayStateId.Playing,GameplayStateId.Playing,false)]
        public void TransitionPolicy_Matrix(GameplayStateId from, GameplayStateId to, bool expected)
        {
            var policy=new GameplayStateTransitionPolicy();Assert.AreEqual(expected,policy.CanTransition(from,to));
        }
    }
}
