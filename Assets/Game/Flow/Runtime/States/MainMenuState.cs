using ZombieWar.GameFlow.Domain;

namespace ZombieWar.GameFlow.States
{
    public sealed class MainMenuState : IGameFlowState
    {
        public GameFlowStateId Id => GameFlowStateId.MainMenu;
        public void Enter() { }
        public void Exit() { }
    }
}
