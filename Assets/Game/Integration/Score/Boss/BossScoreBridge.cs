using System;
using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.Features.Boss.Domain;
using ZombieWar.Features.Boss.Events;
using ZombieWar.Features.Score.Commands;
using ZombieWar.Features.Score.Domain;

namespace ZombieWar.Integration.Score.Boss
{
    public sealed class BossScoreBridge : IStartable, IDisposable
    {
        private readonly IEventSubscriber _events;
        private readonly ICommandBus _commands;
        private IDisposable _subscription;

        public BossScoreBridge(IEventSubscriber events, ICommandBus commands)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public void Start()
        {
            _subscription?.Dispose();
            _subscription = _events.Subscribe<BossDefeatedEvent>(OnDefeated);
        }

        private void OnDefeated(BossDefeatedEvent evt)
        {
            ScoreActionId action = evt.BossId == BossId.BossA ? ScoreActionId.BossADefeated
                : evt.BossId == BossId.BossB ? ScoreActionId.BossBDefeated
                : evt.BossId == BossId.BossC ? ScoreActionId.BossCDefeated
                : ScoreActionId.None;

            if (action != ScoreActionId.None)
                _commands.Send(new AwardScoreCommand(action, evt.EntityId));
        }

        public void Dispose() { _subscription?.Dispose(); _subscription = null; }
    }
}
