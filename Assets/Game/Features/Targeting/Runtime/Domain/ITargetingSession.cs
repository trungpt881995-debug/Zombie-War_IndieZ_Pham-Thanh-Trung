namespace ZombieWar.Features.Targeting.Domain
{
    /// <summary>
    /// Per-owner targeting session. Each Soldier owns one session.
    /// </summary>
    public interface ITargetingSession
    {
        TargetingResult Evaluate(in TargetingContext context);

        void Clear(TargetLossReason reason = TargetLossReason.ManualClear);
    }
}
