namespace ZombieWar.Features.Spawn.Domain
{
    public enum SpawnRejectReason { None = 0, NoSector = 1, InsideCamera = 2, OutsideGameplayBounds = 3, InvalidNavigation = 4, PoolUnavailable = 5, AttemptsExhausted = 6 }
}
