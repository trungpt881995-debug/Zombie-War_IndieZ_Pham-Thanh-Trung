using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Features.Zombie.StateMachine
{
    public sealed class AttackZombieState : IZombieState
    {
        private readonly ZombieStateContext _context;
        private float _decisionRemaining;
        private float _timeUntilNextAttack;
        private float _animationTimeout;
        private bool _waitingForAnimation;
        private bool _impactConsumed;
        public ZombieStateId Id => ZombieStateId.Attack;
        public AttackZombieState(ZombieStateContext context) => _context = context;

        public void Enter()
        {
            _context.Model.SetState(Id);
            _context.Motor.Stop();
            _context.View.SetLocomotionSpeed(0f);
            _decisionRemaining = 0f;
            _timeUntilNextAttack = 0f;
            _waitingForAnimation = false;
            _impactConsumed = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_context.Model.GameplayEnabled) return;
            if (_timeUntilNextAttack > 0f) _timeUntilNextAttack -= deltaTime;

            _decisionRemaining -= deltaTime;
            if (_decisionRemaining <= 0f)
            {
                _decisionRemaining = _context.Model.Definition.AiDecisionInterval;
                if (!_context.TryRefreshOrAcquireTarget(out ZombieTarget refreshed))
                {
                    _context.ChangeState(ZombieStateId.Chase);
                    return;
                }
                ZombiePoint position = _context.Motor.Position;
                ZombiePoint targetPosition = refreshed.Position;
                float exitRange = _context.Model.Definition.AttackRange + _context.Model.Definition.AttackExitRangeBonus;
                if (!ZombieStateContext.InsideRange(in position, in targetPosition, exitRange))
                {
                    _context.ChangeState(ZombieStateId.Chase);
                    return;
                }
            }

            ZombieTarget target = _context.Model.CurrentTarget;
            if (target.IsValid)
            {
                ZombiePoint targetPosition = target.Position;
                _context.View.FaceTarget(in targetPosition, _context.Model.Definition.RotationSpeed, deltaTime);
            }

            if (_waitingForAnimation)
            {
                _animationTimeout -= deltaTime;
                if (_animationTimeout <= 0f)
                {
                    _waitingForAnimation = false;
                    _impactConsumed = false;
                    _timeUntilNextAttack = _context.Model.Definition.AttackInterval;
                }
                return;
            }

            if (_timeUntilNextAttack <= 0f && target.IsValid)
            {
                _waitingForAnimation = true;
                _impactConsumed = false;
                _animationTimeout = _context.Model.Definition.AttackAnimationTimeout;
                _context.View.PlayAttack();
            }
        }

        public void OnAttackImpact()
        {
            if (!_waitingForAnimation || _impactConsumed || !_context.Model.GameplayEnabled) return;
            ZombieTarget target = _context.Model.CurrentTarget;
            if (!target.IsValid) return;
            if (!_context.TargetProvider.TryGetTarget(target.EntityId, out ZombieTarget refreshed) || !refreshed.IsValid) return;
            ZombiePoint position = _context.Motor.Position;
            ZombiePoint targetPosition = refreshed.Position;
            if (!ZombieStateContext.InsideRange(in position, in targetPosition, _context.Model.Definition.AttackRange + _context.Model.Definition.AttackExitRangeBonus)) return;
            _context.Model.SetTarget(in refreshed);
            var request = new ZombieAttackRequest(_context.Model.EntityId, refreshed.EntityId, _context.Model.Definition.AttackDamage);
            _context.AttackPort.TryAttack(in request);
            _impactConsumed = true;
        }

        public void OnAttackAnimationFinished()
        {
            if (!_waitingForAnimation) return;
            _waitingForAnimation = false;
            _impactConsumed = false;
            _timeUntilNextAttack = _context.Model.Definition.AttackInterval;
        }

        public void Exit()
        {
            _waitingForAnimation = false;
            _impactConsumed = false;
        }
    }
}
