using System;
using ZombieWar.GameFlow.Domain;

namespace ZombieWar.GameFlow.Model
{
    public sealed class GameFlowModel
    {
        public GameFlowStateId CurrentState { get; private set; } = GameFlowStateId.None;
        public event Action<GameFlowStateId> StateChanged;

        internal void SetState(GameFlowStateId state)
        {
            if (CurrentState == state) return;
            CurrentState = state;
            StateChanged?.Invoke(state);
        }
    }
}
