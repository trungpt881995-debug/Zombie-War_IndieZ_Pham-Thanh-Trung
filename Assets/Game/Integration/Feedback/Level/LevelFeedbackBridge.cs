using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Services;
using ZombieWar.Features.Level.Events;

namespace ZombieWar.Integration.Feedback.Level
{
    public sealed class LevelFeedbackBridge : IDisposable
    {
        private readonly IEventSubscriber _events;
        private readonly IFeedbackRuntime _feedback;

        private IDisposable _subscription;

        public LevelFeedbackBridge(
            IEventSubscriber events,
            IFeedbackRuntime feedback)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
        }

        public void Start()
        {
            if (_subscription != null)
            {
                return;
            }

            _subscription =
                _events.Subscribe<SoldierGroupLevelChangedEvent>(OnLevelChanged);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void OnLevelChanged(SoldierGroupLevelChangedEvent evt)
        {
            if (evt.Current == evt.Previous)
            {
                return;
            }

            var request =
                new FeedbackRequest(FeedbackId.SoldierGroupLevelUp);

            _feedback.Play(in request);
        }
    }
}
