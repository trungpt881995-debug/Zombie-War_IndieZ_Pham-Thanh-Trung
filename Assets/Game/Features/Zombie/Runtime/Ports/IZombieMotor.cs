using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.Ports
{
    public interface IZombieMotor
    {
        ZombiePoint Position { get; }
        float NormalizedSpeed { get; }
        void Warp(in ZombiePoint position);
        void SetEnabled(bool enabled);
        void MoveTowards(in ZombiePoint target, float speed, float deltaTime);
        void Stop();
    }
}
