using GameplayCore.Entities; using ZombieWar.Features.Boss.Domain; using ZombieWar.Features.Boss.Ports;
namespace ZombieWar.Integration.Boss
{
    public sealed class BossSoldierTargetProvider:IBossTargetProvider,IBossSoldierTargetRegistry
    {
        private const int MaxSoldiers=4;private readonly EntityId[] _ids=new EntityId[MaxSoldiers];private readonly IBossTargetSource[] _sources=new IBossTargetSource[MaxSoldiers];private readonly bool[] _used=new bool[MaxSoldiers];
        public bool Register(EntityId id,IBossTargetSource source){if(source==null)return false;for(int i=0;i<MaxSoldiers;i++)if(_used[i]&&_ids[i].Equals(id)){_sources[i]=source;return true;}for(int i=0;i<MaxSoldiers;i++)if(!_used[i]){_used[i]=true;_ids[i]=id;_sources[i]=source;return true;}return false;}
        public bool Unregister(EntityId id){for(int i=0;i<MaxSoldiers;i++){if(!_used[i]||!_ids[i].Equals(id))continue;_used[i]=false;_ids[i]=default;_sources[i]=null;return true;}return false;} public void Clear(){for(int i=0;i<MaxSoldiers;i++){_used[i]=false;_ids[i]=default;_sources[i]=null;}}
        public bool TryAcquireTarget(in BossPoint bossPosition,out BossTarget target){int best=-1;float bestSqr=float.MaxValue;for(int i=0;i<MaxSoldiers;i++){IBossTargetSource s=_sources[i];if(!_used[i]||s==null||!s.IsActive)continue;BossPoint p=s.Position;float sq=BossPoint.SqrDistanceXZ(in bossPosition,in p);if(sq<bestSqr){bestSqr=sq;best=i;}}if(best<0){target=BossTarget.None;return false;}BossPoint pos=_sources[best].Position;target=BossTarget.From(_ids[best],in pos);return true;}
        public bool TryGetTarget(EntityId id,out BossTarget target){for(int i=0;i<MaxSoldiers;i++){IBossTargetSource s=_sources[i];if(!_used[i]||!_ids[i].Equals(id)||s==null||!s.IsActive)continue;BossPoint p=s.Position;target=BossTarget.From(id,in p);return true;}target=BossTarget.None;return false;}
    }
}
