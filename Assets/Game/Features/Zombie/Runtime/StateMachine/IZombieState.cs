using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.StateMachine
{
    public interface IZombieState
    {
        ZombieStateId Id { get; }
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }
}
