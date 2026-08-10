using ZombieWar.GameFlow.Domain;

namespace ZombieWar.GameFlow.States
{
    public sealed class GameOverState : IGameFlowState
    {
        public GameFlowStateId Id => GameFlowStateId.GameOver;
        public void Enter() { }
        public void Exit() { }
    }
}
