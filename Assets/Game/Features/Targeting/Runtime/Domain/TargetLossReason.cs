namespace ZombieWar.Features.Targeting.Domain
{
    public enum TargetLossReason
    {
        None = 0,
        MissingCandidate = 1,
        EntityIdentityChanged = 2,
        Unregistered = 3,
        NotTargetable = 4,
        OutOfRange = 5,
        ManualClear = 6
    }
}
