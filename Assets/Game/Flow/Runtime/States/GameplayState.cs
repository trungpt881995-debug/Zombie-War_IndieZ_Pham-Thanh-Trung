using ZombieWar.GameFlow.Domain;

namespace ZombieWar.GameFlow.States
{
    public sealed class GameplayState : IGameFlowState
    {
        public GameFlowStateId Id => GameFlowStateId.Gameplay;
        public void Enter() { }
        public void Exit() { }
    }
}
