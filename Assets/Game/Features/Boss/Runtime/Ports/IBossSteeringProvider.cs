using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Features.Boss.Ports
{
    /// <summary>
    /// Optional movement capability used by presentation/state code to face the
    /// next steering point of a path without coupling IBossMotor to Unity NavMesh.
    /// </summary>
    public interface IBossSteeringProvider
    {
        bool TryGetSteeringTarget(out BossPoint target);
    }
}
