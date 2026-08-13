using System;
using GeneralCore.Architecture;
using VContainer.Unity;
using ZombieWar.Features.Score.Commands;
using ZombieWar.Features.Score.Domain;
using ZombieWar.Features.Zombie.Events;

namespace ZombieWar.Integration.Score.Zombie
{
    public sealed class ZombieScoreBridge : IStartable, IDisposable
    {
        private readonly IEventSubscriber _events;
        private readonly ICommandBus _commands;
        private IDisposable _subscription;

        public ZombieScoreBridge(IEventSubscriber events, ICommandBus commands)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public void Start()
        {
            _subscription?.Dispose();
            _subscription = _events.Subscribe<ZombieKilledEvent>(OnKilled);
        }

        private void OnKilled(ZombieKilledEvent evt) =>
            _commands.Send(new AwardScoreCommand(ScoreActionId.NormalZombieDefeated, evt.ZombieId));

        public void Dispose() { _subscription?.Dispose(); _subscription = null; }
    }
}
