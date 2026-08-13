using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Services;
using ZombieWar.Features.GameState.Domain;
using ZombieWar.Features.GameState.Events;
using ZombieWar.Features.GameState.Services;

namespace ZombieWar.Integration.Feedback.GameState
{
    public sealed class GameStateFeedbackBridge : IDisposable
    {
        private readonly IEventSubscriber _events;
        private readonly IFeedbackRuntime _feedback;
        private readonly IGameStateRuntime _gameState;

        private IDisposable _subscription;

        public GameStateFeedbackBridge(
            IEventSubscriber events,
            IFeedbackRuntime feedback,
            IGameStateRuntime gameState)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        }

        public void Start()
        {
            if (_subscription != null)
            {
                return;
            }

            ApplyMode(_gameState.State);

            _subscription =
                _events.Subscribe<GameplayStateChangedEvent>(OnStateChanged);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void OnStateChanged(GameplayStateChangedEvent evt)
        {
            ApplyMode(evt.Current);

            switch (evt.Current)
            {
                case GameplayStateId.GameOver:
                    PlayTerminal(FeedbackId.GameOver);
                    break;

                case GameplayStateId.LevelComplete:
                    PlayTerminal(FeedbackId.LevelComplete);
                    break;

                case GameplayStateId.EndGame:
                    PlayTerminal(FeedbackId.EndGame);
                    break;
            }
        }

        private void ApplyMode(GameplayStateId state)
        {
            switch (state)
            {
                case GameplayStateId.Playing:
                    _feedback.SetMode(FeedbackRuntimeMode.Playing);
                    break;

                case GameplayStateId.Paused:
                    _feedback.SetMode(FeedbackRuntimeMode.Suspended);
                    break;

                case GameplayStateId.GameOver:
                case GameplayStateId.LevelComplete:
                case GameplayStateId.EndGame:
                    _feedback.SetMode(FeedbackRuntimeMode.TerminalDrain);
                    break;

                case GameplayStateId.Inactive:
                default:
                    _feedback.SetMode(FeedbackRuntimeMode.Inactive);
                    break;
            }
        }

        private void PlayTerminal(FeedbackId id)
        {
            var request = new FeedbackRequest(id);
            _feedback.Play(in request);
        }
    }
}
