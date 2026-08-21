using ZombieWar.Features.Boss.Domain;

namespace ZombieWar.Features.Boss.StateMachine
{
    public interface IBossState
    {
        BossStateId Id
        {
            get;
        }
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }
}
