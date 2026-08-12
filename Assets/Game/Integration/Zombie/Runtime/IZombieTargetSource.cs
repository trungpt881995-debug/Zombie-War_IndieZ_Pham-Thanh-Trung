using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Integration.Zombie
{
    public interface IZombieTargetSource
    {
        ZombiePoint Position { get; }
        bool IsActive { get; }
    }
}
