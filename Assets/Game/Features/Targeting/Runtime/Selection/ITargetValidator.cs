using ZombieWar.Features.Targeting.Domain;

namespace ZombieWar.Features.Targeting.Selection
{
    public interface ITargetValidator
    {
        TargetLossReason Validate(in TargetHandle target, in TargetingContext context);
    }
}
