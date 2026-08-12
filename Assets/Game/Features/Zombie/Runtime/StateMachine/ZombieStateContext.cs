using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Model;
using ZombieWar.Features.Zombie.Ports;

namespace ZombieWar.Features.Zombie.StateMachine
{
    public sealed class ZombieStateContext
    {
        public ZombieModel Model { get; }
        public IZombieView View { get; }
        public IZombieMotor Motor { get; }
        public IZombieTargetProvider TargetProvider { get; }
        public IZombieAttackPort AttackPort { get; }
        public IZombieTargetRegistrationPort TargetRegistration { get; }
        public IZombiePoolReturnPort PoolReturn { get; }
        public IZombieFeedbackPort Feedback { get; }
        public IEventBus EventBus { get; }
        public Action<ZombieStateId> ChangeState { get; }

        public ZombieStateContext(
            ZombieModel model,
            IZombieView view,
            IZombieMotor motor,
            IZombieTargetProvider targetProvider,
            IZombieAttackPort attackPort,
            IZombieTargetRegistrationPort targetRegistration,
            IZombiePoolReturnPort poolReturn,
            IZombieFeedbackPort feedback,
            IEventBus eventBus,
            Action<ZombieStateId> changeState)
        {
            Model = model; View = view; Motor = motor; TargetProvider = targetProvider;
            AttackPort = attackPort; TargetRegistration = targetRegistration;
            PoolReturn = poolReturn; Feedback = feedback; EventBus = eventBus;
            ChangeState = changeState;
        }

        public bool TryRefreshOrAcquireTarget(out ZombieTarget target)
        {
            ZombieTarget current = Model.CurrentTarget;
            if (current.IsValid && TargetProvider.TryGetTarget(current.EntityId, out target) && target.IsValid)
            {
                Model.SetTarget(in target);
                return true;
            }

            ZombiePoint position = Motor.Position;
            if (TargetProvider.TryAcquireTarget(in position, out target) && target.IsValid)
            {
                Model.SetTarget(in target);
                return true;
            }

            Model.ClearTarget();
            target = ZombieTarget.None;
            return false;
        }

        public static bool InsideRange(in ZombiePoint a, in ZombiePoint b, float range) =>
            ZombiePoint.SqrDistanceXZ(in a, in b) <= range * range;
    }
}
