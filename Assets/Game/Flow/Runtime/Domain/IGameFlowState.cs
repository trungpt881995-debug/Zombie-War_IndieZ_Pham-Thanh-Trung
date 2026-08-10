namespace ZombieWar.GameFlow.Domain
{
    public interface IGameFlowState
    {
        GameFlowStateId Id { get; }
        void Enter();
        void Exit();
    }
}
