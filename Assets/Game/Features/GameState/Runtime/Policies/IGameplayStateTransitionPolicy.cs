using ZombieWar.Features.GameState.Domain;

namespace ZombieWar.Features.GameState.Policies
{
    public interface IGameplayStateTransitionPolicy
    {
        bool CanTransition(GameplayStateId from, GameplayStateId to);
    }
}
