using System;
using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.StateMachine
{
    public sealed class ZombieStateMachine
    {
        private readonly IZombieState[] _states = new IZombieState[6];
        private IZombieState _current;
        public ZombieStateId CurrentId => _current != null ? _current.Id : ZombieStateId.Inactive;

        public void Register(IZombieState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            _states[(int)state.Id] = state;
        }

        public void Change(ZombieStateId id)
        {
            if (_current != null && _current.Id == id) return;
            IZombieState next = _states[(int)id];
            if (next == null) throw new InvalidOperationException($"Zombie state {id} is not registered.");
            _current?.Exit();
            _current = next;
            _current.Enter();
        }

        public void Tick(float deltaTime) => _current?.Tick(deltaTime);
        public void Clear()
        {
            _current?.Exit();
            _current = null;
        }
    }
}
