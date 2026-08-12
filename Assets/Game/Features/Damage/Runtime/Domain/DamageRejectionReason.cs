namespace ZombieWar.Features.Damage.Domain
{
    public enum DamageRejectionReason
    {
        None = 0,
        TargetMissing = 1,
        TargetNotAlive = 2,
        InvalidAmount = 3
    }
}
