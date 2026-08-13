using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.StateMachine
{
    public sealed class AttackBossState:IBossState
    {
        private readonly BossStateContext _c; private float _decisionRemaining,_cooldown,_animationTimeout; private bool _waiting,_impactConsumed;
        public BossStateId Id=>BossStateId.Attack; public AttackBossState(BossStateContext c){_c=c;}
        public void Enter(){_c.Model.SetState(Id);_c.Motor.Stop();_c.View.SetLocomotionSpeed(0f);_decisionRemaining=0f;_cooldown=0f;_waiting=false;_impactConsumed=false;}
        public void Tick(float dt)
        {
            if(!_c.Model.GameplayEnabled)return;if(_cooldown>0f)_cooldown-=dt;_decisionRemaining-=dt;
            if(_decisionRemaining<=0f){_decisionRemaining=_c.Model.Definition.AiDecisionInterval;if(!_c.TryRefreshOrAcquireTarget(out BossTarget refreshed)){_c.ChangeState(BossStateId.Chase);return;}BossPoint p=_c.Motor.Position,tp=refreshed.Position;float exit=_c.Model.Definition.AttackRange+_c.Model.Definition.AttackExitRangeBonus;if(!BossStateContext.InsideRange(in p,in tp,exit)){_c.ChangeState(BossStateId.Chase);return;}}
            BossTarget target=_c.Model.CurrentTarget;if(target.IsValid){BossPoint tp=target.Position;_c.View.FaceTarget(in tp,_c.Model.Definition.RotationSpeed,dt);}
            if(_waiting){_animationTimeout-=dt;if(_animationTimeout<=0f){_waiting=false;_impactConsumed=false;_cooldown=_c.Model.Definition.AttackCooldown;}return;}
            if(_cooldown<=0f&&target.IsValid){_waiting=true;_impactConsumed=false;_animationTimeout=_c.Model.Definition.AttackAnimationTimeout;_c.View.PlayAttack();}
        }
        public void OnAttackImpact()
        {
            if(!_waiting||_impactConsumed||!_c.Model.GameplayEnabled)return;BossTarget target=_c.Model.CurrentTarget;if(!target.IsValid)return;
            if(!_c.TargetProvider.TryGetTarget(target.EntityId,out BossTarget refreshed)||!refreshed.IsValid)return;BossPoint p=_c.Motor.Position,tp=refreshed.Position;
            if(!BossStateContext.InsideRange(in p,in tp,_c.Model.Definition.AttackRange+_c.Model.Definition.AttackExitRangeBonus))return;_c.Model.SetTarget(in refreshed);
            var request=new BossAttackRequest(_c.Model.EntityId,refreshed.EntityId,_c.Model.Definition.AttackDamage,_c.Model.Definition.AttackType);_c.AttackStrategy.TryExecute(in request);_impactConsumed=true;
        }
        public void OnAttackAnimationFinished(){if(!_waiting)return;_waiting=false;_impactConsumed=false;_cooldown=_c.Model.Definition.AttackCooldown;}
        public void Exit(){_waiting=false;_impactConsumed=false;}
    }
}
