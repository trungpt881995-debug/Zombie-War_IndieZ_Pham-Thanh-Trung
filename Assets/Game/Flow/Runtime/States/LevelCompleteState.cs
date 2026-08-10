using ZombieWar.GameFlow.Domain;

namespace ZombieWar.GameFlow.States
{
    public sealed class LevelCompleteState : IGameFlowState
    {
        public GameFlowStateId Id => GameFlowStateId.LevelComplete;
        public void Enter() { }
        public void Exit() { }
    }
}
