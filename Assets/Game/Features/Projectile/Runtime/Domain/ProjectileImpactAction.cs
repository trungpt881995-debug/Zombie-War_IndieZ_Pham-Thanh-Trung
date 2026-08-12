namespace ZombieWar.Features.Projectile.Domain
{
    public enum ProjectileImpactAction
    {
        Ignore = 0,
        DamageAndComplete = 1,
        DamageAndContinue = 2,
        Complete = 3,
        ExplodeAndComplete = 4
    }
}
