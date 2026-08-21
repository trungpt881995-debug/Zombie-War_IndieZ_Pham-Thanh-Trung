using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.Boss.Controller;

namespace ZombieWar.Features.Boss.Registry
{
    public interface IActiveBossRegistry
    {
        IReadOnlyList < BossController > Active
        {
            get;
        }
        int Count
        {
            get;
        }
        bool Add(BossController boss);
        bool Remove(EntityId id);
        bool Contains(EntityId id);
        void Clear();
    }
}
