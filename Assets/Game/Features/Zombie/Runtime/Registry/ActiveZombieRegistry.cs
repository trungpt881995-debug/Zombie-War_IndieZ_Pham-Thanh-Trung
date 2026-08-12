using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.Zombie.Controller;

namespace ZombieWar.Features.Zombie.Registry
{
    public sealed class ActiveZombieRegistry : IActiveZombieRegistry
    {
        private readonly List<ZombieController> _active = new List<ZombieController>(128);
        private readonly Dictionary<EntityId, int> _indices = new Dictionary<EntityId, int>(128);
        public IReadOnlyList<ZombieController> Active => _active;
        public int Count => _active.Count;

        public bool Add(ZombieController zombie)
        {
            if (zombie == null || _indices.ContainsKey(zombie.EntityId)) return false;
            _indices.Add(zombie.EntityId, _active.Count);
            _active.Add(zombie);
            return true;
        }
        public bool Remove(EntityId entityId)
        {
            if (!_indices.TryGetValue(entityId, out int index)) return false;
            int last = _active.Count - 1;
            _indices.Remove(entityId);
            if (index != last)
            {
                ZombieController moved = _active[last];
                _active[index] = moved;
                _indices[moved.EntityId] = index;
            }
            _active.RemoveAt(last);
            return true;
        }
        public void Clear() { _active.Clear(); _indices.Clear(); }
    }
}
