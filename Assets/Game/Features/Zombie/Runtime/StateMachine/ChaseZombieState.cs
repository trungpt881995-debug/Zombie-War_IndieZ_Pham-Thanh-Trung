using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Ports;

namespace ZombieWar.Features.Zombie.StateMachine
{
    public sealed class ChaseZombieState : IZombieState
    {
        private readonly ZombieStateContext _context;
        private float _decisionRemaining;
        public ZombieStateId Id => ZombieStateId.Chase;
        public ChaseZombieState(ZombieStateContext context) => _context = context;
        public void Enter()
        {
            _context.Model.SetState(Id);
            _context.Motor.SetEnabled(_context.Model.GameplayEnabled);
            _decisionRemaining = 0f;
        }
        public void Tick(float deltaTime)
        {
            if (!_context.Model.GameplayEnabled)
            {
                _context.Motor.Stop();
                _context.View.SetLocomotionSpeed(0f);
                return;
            }

            _decisionRemaining -= deltaTime;
            if (_decisionRemaining <= 0f)
            {
                _decisionRemaining = _context.Model.Definition.AiDecisionInterval;
                if (!_context.TryRefreshOrAcquireTarget(out ZombieTarget decisionTarget))
                {
                    _context.Motor.Stop();
                    _context.View.SetLocomotionSpeed(0f);
                    return;
                }

                ZombiePoint position = _context.Motor.Position;
                ZombiePoint targetPosition = decisionTarget.Position;
                if (ZombieStateContext.InsideRange(in position, in targetPosition, _context.Model.Definition.AttackRange))
                {
                    _context.ChangeState(ZombieStateId.Attack);
                    return;
                }
            }

            ZombieTarget target = _context.Model.CurrentTarget;
            if (!target.IsValid) return;
            ZombiePoint destination = target.Position;
            _context.Motor.MoveTowards(in destination, _context.Model.Definition.MoveSpeed, deltaTime);
            _context.View.SetLocomotionSpeed(_context.Motor.NormalizedSpeed);

            // NavMesh movement may steer around corners/obstacles. When the motor exposes
            // a steering point, face that point instead of looking through the obstacle at
            // the final Soldier destination. Non-NavMesh motors keep the old behaviour.
            ZombiePoint facingTarget = destination;
            if (_context.Motor is IZombieSteeringProvider steeringProvider &&
                steeringProvider.TryGetSteeringTarget(out ZombiePoint steeringTarget))
            {
                facingTarget = steeringTarget;
            }

            _context.View.FaceTarget(
                in facingTarget,
                _context.Model.Definition.RotationSpeed,
                deltaTime);
        }
        public void Exit() { _context.Motor.Stop(); _context.View.SetLocomotionSpeed(0f); }
    }
}
