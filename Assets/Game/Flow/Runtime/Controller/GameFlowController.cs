using GeneralCore.Architecture;
using GameplayCore.Session;
using GameplayCore.Time;
using ZombieWar.GameFlow.Domain;
using ZombieWar.GameFlow.Model;
using ZombieWar.GameFlow.StateMachine;
using ZombieWar.GameFlow.View;

namespace ZombieWar.GameFlow.Controller
{
    public sealed class GameFlowController : IController
    {
        private readonly GameFlowModel _model;
        private readonly GameFlowStateMachine _stateMachine;
        private readonly IGameFlowView _view;
        private readonly IGameplaySession _session;
        private readonly IGameplayClockControl _clock;

        public GameFlowController(GameFlowModel model, GameFlowStateMachine stateMachine, IGameFlowView view, IGameplaySession session, IGameplayClockControl clock)
        {
            _model = model;
            _stateMachine = stateMachine;
            _view = view;
            _session = session;
            _clock = clock;
            _model.StateChanged += _view.Render;
        }

        public void Initialize()
        {
            _clock.Reset();
            _session.Prepare();
            _stateMachine.ChangeState(GameFlowStateId.Boot);
            _stateMachine.ChangeState(GameFlowStateId.MainMenu);
        }

        public void GoToMainMenu()
        {
            if (_session.State != GameplaySessionState.Uninitialized && _session.State != GameplaySessionState.Stopped)
                _session.Stop();
            _clock.SetPaused(false);
            _stateMachine.ChangeState(GameFlowStateId.MainMenu);
        }

        public void BeginLoading()
        {
            // Loading starts a fresh level attempt. This is especially important for
            // Replay from Pause, where the previous GameplaySession would otherwise
            // remain Paused when BeginGameplay() is called.
            if (_session.State != GameplaySessionState.Uninitialized &&
                _session.State != GameplaySessionState.Stopped)
            {
                _session.Stop();
            }

            _clock.SetPaused(false);
            _stateMachine.ChangeState(GameFlowStateId.Loading);
        }

        public void BeginGameplay()
        {
            if (_session.State == GameplaySessionState.Uninitialized || _session.State == GameplaySessionState.Stopped || _session.State == GameplaySessionState.Completed || _session.State == GameplaySessionState.Failed)
                _session.Prepare();
            if (_session.State == GameplaySessionState.Preparing) _session.Start();
            _clock.SetPaused(false);
            _stateMachine.ChangeState(GameFlowStateId.Gameplay);
        }

        public void Pause()
        {
            if (_session.State == GameplaySessionState.Running) _session.Pause();
            _clock.SetPaused(true);
            _stateMachine.ChangeState(GameFlowStateId.Paused);
        }

        public void Resume()
        {
            if (_session.State == GameplaySessionState.Paused) _session.Resume();
            _clock.SetPaused(false);
            _stateMachine.ChangeState(GameFlowStateId.Gameplay);
        }

        public void LevelComplete()
        {
            if (_session.State == GameplaySessionState.Running || _session.State == GameplaySessionState.Paused) _session.Complete();
            _clock.SetPaused(true);
            _stateMachine.ChangeState(GameFlowStateId.LevelComplete);
        }

        public void GameOver()
        {
            if (_session.State == GameplaySessionState.Running || _session.State == GameplaySessionState.Paused) _session.Fail();
            _clock.SetPaused(true);
            _stateMachine.ChangeState(GameFlowStateId.GameOver);
        }

        public void EndGame()
        {
            if (_session.State == GameplaySessionState.Running || _session.State == GameplaySessionState.Paused) _session.Complete();
            _clock.SetPaused(true);
            _stateMachine.ChangeState(GameFlowStateId.EndGame);
        }
    }
}
