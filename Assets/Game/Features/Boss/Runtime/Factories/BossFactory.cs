using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Boss.Controller;
using ZombieWar.Features.Boss.Model;
using ZombieWar.Features.Boss.Ports;
using ZombieWar.Features.Boss.Strategies;

namespace ZombieWar.Features.Boss.Factories
{
    public sealed class BossFactory : IBossFactory
    {
        private readonly IEntityIdGenerator _ids;
        private readonly IBossTargetProvider _targets;
        private readonly IBossAttackPort _attack;
        private readonly IBossFeedbackPort _feedback;
        private readonly IEventBus _events;
        public BossFactory(IEntityIdGenerator ids, IBossTargetProvider targets, IBossAttackPort attack, IBossFeedbackPort feedback,
        IEventBus events)
        {
            _ids = ids;
            _targets = targets;
            _attack = attack;
            _feedback = feedback ?? NullBossFeedbackPort.Instance;
            _events = events;
        }
        public BossController Create(IBossView view, IBossMotor motor, IBossHealthPort health, IBossTargetRegistrationPort registration,
        IBossPoolReturnPort poolReturn) => new BossController(_ids, new BossModel(), view, motor, health, _targets, new BasicMeleeBossAttackStrategy(_attack),
        registration, poolReturn, _feedback, _events);
    }
}
