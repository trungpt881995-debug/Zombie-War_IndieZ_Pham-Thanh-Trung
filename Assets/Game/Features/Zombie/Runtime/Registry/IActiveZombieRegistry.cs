using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.Zombie.Controller;

namespace ZombieWar.Features.Zombie.Registry
{
    public interface IActiveZombieRegistry
    {
        IReadOnlyList<ZombieController> Active { get; }
        int Count { get; }
        bool Add(ZombieController zombie);
        bool Remove(EntityId entityId);
        void Clear();
    }
}
