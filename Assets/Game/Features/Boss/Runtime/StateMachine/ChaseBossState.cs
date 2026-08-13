using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.StateMachine
{
    public sealed class ChaseBossState:IBossState
    {
        private readonly BossStateContext _c; private float _decisionRemaining; public BossStateId Id=>BossStateId.Chase; public ChaseBossState(BossStateContext c){_c=c;}
        public void Enter(){_c.Model.SetState(Id);_decisionRemaining=0f;}
        public void Tick(float dt)
        {
            if(!_c.Model.GameplayEnabled)return;_decisionRemaining-=dt;
            if(_decisionRemaining<=0f){_decisionRemaining=_c.Model.Definition.AiDecisionInterval;if(_c.TryRefreshOrAcquireTarget(out BossTarget t)){BossPoint p=_c.Motor.Position,tp=t.Position;if(BossStateContext.InsideRange(in p,in tp,_c.Model.Definition.AttackRange)){_c.ChangeState(BossStateId.Attack);return;}}}
            BossTarget target=_c.Model.CurrentTarget;if(!target.IsValid){_c.Motor.Stop();_c.View.SetLocomotionSpeed(0f);return;}BossPoint dest=target.Position;_c.Motor.MoveTowards(in dest,_c.Model.Definition.MoveSpeed,dt);_c.View.SetLocomotionSpeed(_c.Motor.NormalizedSpeed);_c.View.FaceTarget(in dest,_c.Model.Definition.RotationSpeed,dt);
        }
        public void Exit(){_c.Motor.Stop();_c.View.SetLocomotionSpeed(0f);}
    }
}
