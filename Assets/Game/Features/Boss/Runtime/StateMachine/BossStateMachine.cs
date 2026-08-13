using System; using System.Collections.Generic; using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.StateMachine
{
    public sealed class BossStateMachine
    {
        private readonly Dictionary<BossStateId,IBossState> _states=new Dictionary<BossStateId,IBossState>(5); private IBossState _current;
        public BossStateId CurrentId=>_current!=null?_current.Id:BossStateId.Inactive;
        public void Register(IBossState state){if(state==null)throw new ArgumentNullException(nameof(state));_states[state.Id]=state;}
        public void Change(BossStateId id){if(_current!=null&&_current.Id==id)return;if(!_states.TryGetValue(id,out IBossState next))throw new InvalidOperationException($"Boss state is not registered: {id}");_current?.Exit();_current=next;_current.Enter();}
        public void Tick(float dt)=>_current?.Tick(dt); public void Clear(){_current?.Exit();_current=null;}
    }
}
