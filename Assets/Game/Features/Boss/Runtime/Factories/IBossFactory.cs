using ZombieWar.Features.Boss.Controller; using ZombieWar.Features.Boss.Ports;
namespace ZombieWar.Features.Boss.Factories { public interface IBossFactory { BossController Create(IBossView view,IBossMotor motor,IBossHealthPort health,IBossTargetRegistrationPort registration,IBossPoolReturnPort poolReturn); } }
