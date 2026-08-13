namespace ZombieWar.Features.GameState.Domain
{
    public enum GameplayStateTransitionReason
    {
        None = 0,
        GameFlowGameplayReady = 1,
        UserPauseRequested = 10,
        UserResumeRequested = 11,
        SoldierGroupDefeated = 20,
        GameLevelCompleted = 30,
        GameCompleted = 31,
        GameFlowDeactivated = 40
    }
}
