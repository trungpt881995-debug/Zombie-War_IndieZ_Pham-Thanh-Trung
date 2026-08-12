using ZombieWar.Features.Targeting.Domain;

namespace ZombieWar.Features.Targeting.Selection
{
    /// <summary>
    /// Planar XZ distance for top-down ground combat. Uses squared distance to
    /// avoid square-root work in the hot path.
    /// </summary>
    public sealed class PlanarXZDistanceMetric : IDistanceMetric
    {
        public float SqrDistance(in TargetPoint from, in TargetPoint to)
        {
            float x = to.X - from.X;
            float z = to.Z - from.Z;

            return x * x + z * z;
        }
    }
}
