using ZombieWar.Features.Zombie.Controller;
using ZombieWar.Features.Zombie.Ports;

namespace ZombieWar.Features.Zombie.Factories
{
    public interface IZombieFactory
    {
        ZombieController Create(
            IZombieView view,
            IZombieMotor motor,
            IZombieHealthPort health,
            IZombieTargetRegistrationPort targetRegistration,
            IZombiePoolReturnPort poolReturn);
    }
}
