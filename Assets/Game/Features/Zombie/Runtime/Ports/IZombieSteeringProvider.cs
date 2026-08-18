using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.Ports
{
    /// <summary>
    /// Optional movement capability used by presentation to face the next steering point
    /// instead of always facing the final destination. Motors that do not provide path
    /// steering can simply omit this interface.
    /// </summary>
    public interface IZombieSteeringProvider
    {
        bool TryGetSteeringTarget(out ZombiePoint target);
    }
}
