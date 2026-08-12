namespace ZombieWar.Features.Projectile.Domain
{
    public enum ProjectileEndReason
    {
        None = 0,
        Hit = 1,
        MaxRangeReached = 2,
        LifetimeExpired = 3,
        GroundExplosion = 4,
        EnvironmentHit = 5,
        Cancelled = 6
    }
}
