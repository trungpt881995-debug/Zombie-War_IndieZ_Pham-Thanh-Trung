using ZombieWar.Features.Boss.Domain;
using ZombieWar.Features.Boss.Ports;

namespace ZombieWar.Features.Boss.StateMachine
{
    public sealed class ChaseBossState : IBossState
    {
        private readonly BossStateContext _c;
        private float _decisionRemaining;
        public BossStateId Id => BossStateId.Chase;
        public ChaseBossState(BossStateContext c)
        {
            _c = c;
        }
        public void Enter()
        {
            _c.Model.SetState(Id);
            _c.Motor.SetEnabled(_c.Model.GameplayEnabled);
            _decisionRemaining = 0f;
        }
        public void Tick(float dt)
        {
            if (!_c.Model.GameplayEnabled)
            {
                _c.Motor.Stop();
                _c.View.SetLocomotionSpeed(0f);
                return;
            }
            _decisionRemaining -= dt;
            if (_decisionRemaining <= 0f)
            {
                _decisionRemaining = _c.Model.Definition.AiDecisionInterval;
                if (_c.TryRefreshOrAcquireTarget(out BossTarget decisionTarget))
                {
                    BossPoint position = _c.Motor.Position;
                    BossPoint targetPosition = decisionTarget.Position;
                    if (BossStateContext.InsideRange(in position, in targetPosition, _c.Model.Definition.AttackRange))
                    {
                        _c.ChangeState(BossStateId.Attack);
                        return;
                    }
                }
            }
            BossTarget target = _c.Model.CurrentTarget;
            if (!target.IsValid)
            {
                _c.Motor.Stop();
                _c.View.SetLocomotionSpeed(0f);
                return;
            }
            BossPoint destination = target.Position;
            _c.Motor.MoveTowards(in destination, _c.Model.Definition.MoveSpeed, dt);
            _c.View.SetLocomotionSpeed(_c.Motor.NormalizedSpeed);
            // A NavMesh path can turn around buildings/walls. Face the next steering
            // corner during Chase instead of visually looking through the obstacle at the
            // final Soldier target. Other IBossMotor implementations keep old behaviour.
            BossPoint facingTarget = destination;
            if (_c.Motor is IBossSteeringProvider steeringProvider && steeringProvider.TryGetSteeringTarget(out BossPoint steeringTarget))
            {
                facingTarget = steeringTarget;
            }
            _c.View.FaceTarget(in facingTarget, _c.Model.Definition.RotationSpeed, dt);
        }
        public void Exit()
        {
            _c.Motor.Stop();
            _c.View.SetLocomotionSpeed(0f);
        }
    }
}
