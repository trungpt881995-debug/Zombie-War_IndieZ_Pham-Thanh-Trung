using ZombieWar.GameFlow.Domain;

namespace ZombieWar.GameFlow.States
{
    public sealed class LoadingState : IGameFlowState
    {
        public GameFlowStateId Id => GameFlowStateId.Loading;
        public void Enter() { }
        public void Exit() { }
    }
}
