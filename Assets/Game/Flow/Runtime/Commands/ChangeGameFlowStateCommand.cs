using GameplayCore.Commands;
using ZombieWar.GameFlow.Domain;

namespace ZombieWar.GameFlow.Commands
{
    public readonly struct ChangeGameFlowStateCommand : IGameplayCommand
    {
        public GameFlowStateId Target { get; }
        public ChangeGameFlowStateCommand(GameFlowStateId target) => Target = target;
    }
}
