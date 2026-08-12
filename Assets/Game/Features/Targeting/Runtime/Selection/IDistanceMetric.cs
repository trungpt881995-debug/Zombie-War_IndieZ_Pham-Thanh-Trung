using ZombieWar.Features.Targeting.Domain;

namespace ZombieWar.Features.Targeting.Selection
{
    /// <summary>
    /// Strategy Pattern: distance calculation can be replaced without modifying
    /// target selection or retention code.
    /// </summary>
    public interface IDistanceMetric
    {
        float SqrDistance(in TargetPoint from, in TargetPoint to);
    }
}
