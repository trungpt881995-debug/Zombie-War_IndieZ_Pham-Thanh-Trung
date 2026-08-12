using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.Targeting.Domain;

namespace ZombieWar.Features.Targeting.Registry
{
    /// <summary>
    /// Allocation-free-on-read registry with O(1) register/contains and
    /// swap-remove unregister. No LINQ and no per-query collection creation.
    /// </summary>
    public sealed class TargetRegistry : ITargetRegistry
    {
        private readonly List<ITargetCandidate> _targets = new List<ITargetCandidate>(64);

        // Keep registered identity separate from the mutable candidate reference.
        private readonly List<EntityId> _registeredIds = new List<EntityId>(64);

        private readonly Dictionary<EntityId, int> _indices = new Dictionary<EntityId, int>(64);

        public IReadOnlyList<ITargetCandidate> ActiveTargets => _targets;
        public int Count => _targets.Count;

        public bool Register(ITargetCandidate target)
        {
            if (target == null)
                return false;

            EntityId id = target.EntityId;

            if (_indices.ContainsKey(id))
                return false;

            int index = _targets.Count;

            _targets.Add(target);
            _registeredIds.Add(id);
            _indices.Add(id, index);

            return true;
        }

        public bool Unregister(EntityId entityId)
        {
            if (!_indices.TryGetValue(entityId, out int index))
                return false;

            int lastIndex = _targets.Count - 1;
            _indices.Remove(entityId);

            if (index != lastIndex)
            {
                ITargetCandidate movedTarget = _targets[lastIndex];
                EntityId movedRegisteredId = _registeredIds[lastIndex];

                _targets[index] = movedTarget;
                _registeredIds[index] = movedRegisteredId;
                _indices[movedRegisteredId] = index;
            }

            _targets.RemoveAt(lastIndex);
            _registeredIds.RemoveAt(lastIndex);

            return true;
        }

        public bool Contains(EntityId entityId)
        {
            return _indices.ContainsKey(entityId);
        }

        public void Clear()
        {
            _targets.Clear();
            _registeredIds.Clear();
            _indices.Clear();
        }
    }
}
