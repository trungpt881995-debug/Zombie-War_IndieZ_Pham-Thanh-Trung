using GameplayCore.Entities;
using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Ports;

namespace ZombieWar.Integration.Zombie
{
    public sealed class ZombieSoldierTargetProvider : IZombieTargetProvider, IZombieSoldierTargetRegistry
    {
        private const int MaxSoldiers = 4;
        private readonly EntityId[] _ids = new EntityId[MaxSoldiers];
        private readonly IZombieTargetSource[] _sources = new IZombieTargetSource[MaxSoldiers];
        private readonly bool[] _used = new bool[MaxSoldiers];

        public bool Register(EntityId soldierId, IZombieTargetSource source)
        {
            if (source == null) return false;
            for (int i = 0; i < MaxSoldiers; i++)
                if (_used[i] && _ids[i].Equals(soldierId)) { _sources[i] = source; return true; }
            for (int i = 0; i < MaxSoldiers; i++)
                if (!_used[i]) { _used[i] = true; _ids[i] = soldierId; _sources[i] = source; return true; }
            return false;
        }
        public bool Unregister(EntityId soldierId)
        {
            for (int i = 0; i < MaxSoldiers; i++)
            {
                if (!_used[i] || !_ids[i].Equals(soldierId)) continue;
                _used[i] = false; _ids[i] = default; _sources[i] = null; return true;
            }
            return false;
        }
        public void Clear()
        {
            for (int i = 0; i < MaxSoldiers; i++) { _used[i] = false; _ids[i] = default; _sources[i] = null; }
        }
        public bool TryAcquireTarget(in ZombiePoint zombiePosition, out ZombieTarget target)
        {
            int best = -1; float bestSqr = float.MaxValue;
            for (int i = 0; i < MaxSoldiers; i++)
            {
                IZombieTargetSource source = _sources[i];
                if (!_used[i] || source == null || !source.IsActive) continue;
                ZombiePoint p = source.Position;
                float sqr = ZombiePoint.SqrDistanceXZ(in zombiePosition, in p);
                if (sqr < bestSqr) { bestSqr = sqr; best = i; }
            }
            if (best < 0) { target = ZombieTarget.None; return false; }
            ZombiePoint position = _sources[best].Position;
            target = ZombieTarget.From(_ids[best], in position);
            return true;
        }
        public bool TryGetTarget(EntityId entityId, out ZombieTarget target)
        {
            for (int i = 0; i < MaxSoldiers; i++)
            {
                IZombieTargetSource source = _sources[i];
                if (!_used[i] || !_ids[i].Equals(entityId) || source == null || !source.IsActive) continue;
                ZombiePoint position = source.Position;
                target = ZombieTarget.From(entityId, in position);
                return true;
            }
            target = ZombieTarget.None;
            return false;
        }
    }
}
