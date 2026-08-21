namespace ZombieWar.Features.Spawn.Domain
{
    public enum SpawnStopReason 
    { 
        None = 0, 
        BossPhase = 1, 
        LevelComplete = 2, 
        GameOver = 3, 
        LevelTransition = 4, 
        Manual = 5 
    }
}
