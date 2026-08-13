using ZombieWar.Features.GameState.Domain;

namespace ZombieWar.Features.GameState.Policies
{
    public sealed class GameplayStateTransitionPolicy : IGameplayStateTransitionPolicy
    {
        public bool CanTransition(GameplayStateId from, GameplayStateId to)
        {
            if (from == to) return false;

            switch (from)
            {
                case GameplayStateId.Inactive:
                    return to == GameplayStateId.Playing;

                case GameplayStateId.Playing:
                    return to == GameplayStateId.Paused ||
                           to == GameplayStateId.GameOver ||
                           to == GameplayStateId.LevelComplete ||
                           to == GameplayStateId.EndGame ||
                           to == GameplayStateId.Inactive;

                case GameplayStateId.Paused:
                    return to == GameplayStateId.Playing ||
                           to == GameplayStateId.GameOver ||
                           to == GameplayStateId.LevelComplete ||
                           to == GameplayStateId.EndGame ||
                           to == GameplayStateId.Inactive;

                case GameplayStateId.GameOver:
                case GameplayStateId.LevelComplete:
                case GameplayStateId.EndGame:
                    return to == GameplayStateId.Inactive;

                default:
                    return false;
            }
        }
    }
}
