using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.StateMachine
{
    public sealed class HitBossState:IBossState
    {
        private readonly BossStateContext _c; private float _remaining; public BossStateId Id=>BossStateId.Hit; public HitBossState(BossStateContext c){_c=c;}
        public void Enter(){_c.Model.SetState(Id);_c.Model.BeginHitReactionCooldown();_c.Motor.Stop();_c.View.SetLocomotionSpeed(0f);_c.View.PlayHit();_remaining=_c.Model.Definition.HitReactionDuration;}
        public void Tick(float dt){if(!_c.Model.GameplayEnabled)return;_remaining-=dt;if(_remaining<=0f)_c.ChangeState(BossStateId.Chase);} public void Finish(){if(_c.Model.State==Id)_c.ChangeState(BossStateId.Chase);} public void Exit(){}
    }
}
