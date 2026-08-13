using System;
using GeneralCore.Architecture;
using ZombieWar.Features.GameState.Commands;
using ZombieWar.Features.Health.Events;

namespace ZombieWar.Integration.GameState.Soldier
{
    public sealed class SoldierDefeatGameStateBridge : IDisposable
    {
        private readonly IEventSubscriber _events;
        private readonly ICommandBus _commands;
        private readonly IGameStateSoldierBinding _binding;
        private IDisposable _subscription;

        public SoldierDefeatGameStateBridge(
            IEventSubscriber events,
            ICommandBus commands,
            IGameStateSoldierBinding binding)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _binding = binding ?? throw new ArgumentNullException(nameof(binding));
        }

        public void Start()
        {
            if (_subscription != null) return;
            _subscription = _events.Subscribe<HealthDepletedEvent>(OnHealthDepleted);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void OnHealthDepleted(HealthDepletedEvent evt)
        {
            if (!_binding.IsBound) return;
            if (!_binding.GroupId.Equals(evt.OwnerId)) return;
            _commands.Send(new EnterGameOverCommand());
        }
    }
}
