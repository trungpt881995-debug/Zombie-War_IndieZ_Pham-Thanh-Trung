using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.Ports
{
    public interface IZombieAttackPort
    {
        bool TryAttack(in ZombieAttackRequest request);
    }
}
