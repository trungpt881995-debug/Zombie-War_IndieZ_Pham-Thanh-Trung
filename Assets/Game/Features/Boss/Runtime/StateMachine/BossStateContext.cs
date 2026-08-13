using System; using GeneralCore.Architecture; using ZombieWar.Features.Boss.Domain; using ZombieWar.Features.Boss.Model; using ZombieWar.Features.Boss.Ports; using ZombieWar.Features.Boss.Strategies;
namespace ZombieWar.Features.Boss.StateMachine
{
    public sealed class BossStateContext
    {
        public BossModel Model{get;} public IBossView View{get;} public IBossMotor Motor{get;} public IBossTargetProvider TargetProvider{get;} public IBossAttackStrategy AttackStrategy{get;}
        public IBossTargetRegistrationPort TargetRegistration{get;} public IBossPoolReturnPort PoolReturn{get;} public IBossFeedbackPort Feedback{get;} public IEventBus EventBus{get;} public Action<BossStateId> ChangeState{get;}
        public BossStateContext(BossModel model,IBossView view,IBossMotor motor,IBossTargetProvider targetProvider,IBossAttackStrategy attackStrategy,IBossTargetRegistrationPort targetRegistration,IBossPoolReturnPort poolReturn,IBossFeedbackPort feedback,IEventBus eventBus,Action<BossStateId> changeState)
        {Model=model??throw new ArgumentNullException(nameof(model));View=view??throw new ArgumentNullException(nameof(view));Motor=motor??throw new ArgumentNullException(nameof(motor));TargetProvider=targetProvider??throw new ArgumentNullException(nameof(targetProvider));AttackStrategy=attackStrategy??throw new ArgumentNullException(nameof(attackStrategy));TargetRegistration=targetRegistration??throw new ArgumentNullException(nameof(targetRegistration));PoolReturn=poolReturn??throw new ArgumentNullException(nameof(poolReturn));Feedback=feedback??NullBossFeedbackPort.Instance;EventBus=eventBus??throw new ArgumentNullException(nameof(eventBus));ChangeState=changeState??throw new ArgumentNullException(nameof(changeState));}
        public bool TryRefreshOrAcquireTarget(out BossTarget target)
        {
            BossTarget current=Model.CurrentTarget;
            if(current.IsValid&&TargetProvider.TryGetTarget(current.EntityId,out target)&&target.IsValid){Model.SetTarget(in target);return true;}
            BossPoint p=Motor.Position; if(TargetProvider.TryAcquireTarget(in p,out target)&&target.IsValid){Model.SetTarget(in target);return true;} Model.ClearTarget();target=BossTarget.None;return false;
        }
        public static bool InsideRange(in BossPoint a,in BossPoint b,float range)=>BossPoint.SqrDistanceXZ(in a,in b)<=range*range;
    }
}
