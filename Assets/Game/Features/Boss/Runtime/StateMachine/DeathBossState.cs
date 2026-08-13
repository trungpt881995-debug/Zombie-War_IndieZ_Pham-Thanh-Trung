using ZombieWar.Features.Boss.Domain; using ZombieWar.Features.Boss.Events;
namespace ZombieWar.Features.Boss.StateMachine
{
    public sealed class DeathBossState:IBossState
    {
        private readonly BossStateContext _c; private float _remaining; private bool _defeatPublished; public BossStateId Id=>BossStateId.Death; public DeathBossState(BossStateContext c){_c=c;}
        public void Enter()
        {
            _c.Model.SetState(Id);_c.Model.SetTargetable(false);_c.Model.ClearTarget();_c.TargetRegistration.Unregister(_c.Model.EntityId);_c.Motor.Stop();_c.Motor.SetEnabled(false);_c.View.SetLocomotionSpeed(0f);_c.View.SetGameplayCollisionEnabled(false);
            BossPoint p=_c.View.Position;_c.Feedback.OnDeath(_c.Model.Definition.Id,_c.Model.EntityId,in p);
            if(!_defeatPublished){_c.EventBus.Publish(new BossDefeatedEvent(_c.Model.Definition.Id,_c.Model.EntityId,_c.Model.LastDamageSource,_c.Model.Definition.RewardScore));_defeatPublished=true;}
            _c.View.PlayDeath();_remaining=_c.Model.Definition.DeathDuration;if(_remaining<=0f)Complete();
        }
        public void Tick(float dt){if(_c.Model.ReturnRequested)return;_remaining-=dt;if(_remaining<=0f)Complete();}
        public void OnDeathAnimationFinished()=>Complete();
        private void Complete(){if(_c.Model.ReturnRequested)return;_c.Model.MarkReturnRequested();_c.EventBus.Publish(new BossReleasedEvent(_c.Model.Definition.Id,_c.Model.EntityId,BossReleaseReason.Death));_c.PoolReturn.Return(_c.Model.EntityId,BossReleaseReason.Death);}
        public void Exit(){} public void ResetForReuse(){_remaining=0f;_defeatPublished=false;}
    }
}
