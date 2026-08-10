using ZombieWar.GameFlow.Domain;

namespace ZombieWar.GameFlow.States
{
    public sealed class BootState : IGameFlowState
    {
        public GameFlowStateId Id => GameFlowStateId.Boot;
        public void Enter() { }
        public void Exit() { }
    }
}
