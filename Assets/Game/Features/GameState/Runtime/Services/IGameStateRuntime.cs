using ZombieWar.Features.GameState.Domain;

namespace ZombieWar.Features.GameState.Services
{
    public interface IGameStateRuntime
    {
        bool IsInitialized { get; }
        GameplayStateId State { get; }
        GameplayStateSnapshot Snapshot { get; }

        GameplayStateTransitionResult BeginGameplay();
        GameplayStateTransitionResult Pause();
        GameplayStateTransitionResult Resume();
        GameplayStateTransitionResult EnterGameOver();
        GameplayStateTransitionResult EnterLevelComplete();
        GameplayStateTransitionResult EnterEndGame();
        GameplayStateTransitionResult Deactivate();
    }
}
