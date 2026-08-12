using GeneralCore.Architecture;
using GameplayCore.Entities;
using ZombieWar.Features.Zombie.Controller;
using ZombieWar.Features.Zombie.Model;
using ZombieWar.Features.Zombie.Ports;

namespace ZombieWar.Features.Zombie.Factories
{
    public sealed class ZombieFactory : IZombieFactory
    {
        private readonly IEntityIdGenerator _ids;
        private readonly IZombieTargetProvider _targetProvider;
        private readonly IZombieAttackPort _attackPort;
        private readonly IZombieFeedbackPort _feedback;
        private readonly IEventBus _events;

        public ZombieFactory(
            IEntityIdGenerator ids,
            IZombieTargetProvider targetProvider,
            IZombieAttackPort attackPort,
            IZombieFeedbackPort feedback,
            IEventBus events)
        {
            _ids = ids; _targetProvider = targetProvider; _attackPort = attackPort;
            _feedback = feedback ?? NullZombieFeedbackPort.Instance; _events = events;
        }

        public ZombieController Create(
            IZombieView view,
            IZombieMotor motor,
            IZombieHealthPort health,
            IZombieTargetRegistrationPort targetRegistration,
            IZombiePoolReturnPort poolReturn)
        {
            return new ZombieController(
                _ids, new ZombieModel(), view, motor, health,
                _targetProvider, _attackPort, targetRegistration, poolReturn, _feedback, _events);
        }
    }
}
