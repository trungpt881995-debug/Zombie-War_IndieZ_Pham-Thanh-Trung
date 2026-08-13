namespace ZombieWar.Features.GameState.Domain
{
    public enum GameplayStateTransitionFailure
    {
        None = 0,
        NotInitialized = 1,
        SameState = 2,
        InvalidTransition = 3
    }
}
