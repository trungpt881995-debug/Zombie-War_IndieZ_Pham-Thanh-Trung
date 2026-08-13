using System.Collections.Generic; using GameplayCore.Entities; using ZombieWar.Features.Boss.Controller;
namespace ZombieWar.Features.Boss.Registry
{
    public sealed class ActiveBossRegistry:IActiveBossRegistry
    {
        private readonly List<BossController> _active=new List<BossController>(2); private readonly Dictionary<EntityId,int> _index=new Dictionary<EntityId,int>(2); public IReadOnlyList<BossController> Active=>_active; public int Count=>_active.Count;
        public bool Add(BossController boss){if(boss==null||_index.ContainsKey(boss.EntityId))return false;_index.Add(boss.EntityId,_active.Count);_active.Add(boss);return true;}
        public bool Remove(EntityId id){if(!_index.TryGetValue(id,out int index))return false;int last=_active.Count-1;BossController moved=_active[last];_active[index]=moved;_active.RemoveAt(last);_index.Remove(id);if(index<last)_index[moved.EntityId]=index;return true;}
        public bool Contains(EntityId id)=>_index.ContainsKey(id); public void Clear(){_active.Clear();_index.Clear();}
    }
}
