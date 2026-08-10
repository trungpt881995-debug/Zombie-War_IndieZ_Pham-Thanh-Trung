using ZombieWar.GameFlow.Domain;

namespace ZombieWar.GameFlow.States
{
    public sealed class EndGameState : IGameFlowState
    {
        public GameFlowStateId Id => GameFlowStateId.EndGame;
        public void Enter() { }
        public void Exit() { }
    }
}
