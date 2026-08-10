using ZombieWar.GameFlow.Domain;

namespace ZombieWar.GameFlow.States
{
    public sealed class PausedState : IGameFlowState
    {
        public GameFlowStateId Id => GameFlowStateId.Paused;
        public void Enter() { }
        public void Exit() { }
    }
}
