using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.Targeting.Domain;

namespace ZombieWar.Features.Targeting.Registry
{
    /// <summary>
    /// Registry Pattern: active target lookup is abstracted from Soldier targeting.
    /// Zombie/Boss lifecycle registers and unregisters candidates.
    /// </summary>
    public interface ITargetRegistry
    {
        IReadOnlyList<ITargetCandidate> ActiveTargets { get; }
        int Count { get; }

        bool Register(ITargetCandidate target);
        bool Unregister(EntityId entityId);
        bool Contains(EntityId entityId);
        void Clear();
    }
}
