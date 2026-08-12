using System;
using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;

namespace ZombieWar.Integration.Weapon
{
    public sealed class WeaponMuzzleRegistry : IWeaponMuzzleRegistry, IWeaponMuzzleProvider
    {
        private readonly Dictionary<EntityId, IWeaponMuzzleSource> _sources =
            new Dictionary<EntityId, IWeaponMuzzleSource>(4);

        public void Register(EntityId ownerId, IWeaponMuzzleSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _sources[ownerId] = source;
        }
        public void Unregister(EntityId ownerId) => _sources.Remove(ownerId);
        public void Clear() => _sources.Clear();

        public bool TryGetMuzzle(EntityId ownerId, out WeaponMuzzle muzzle)
        {
            if (!_sources.TryGetValue(ownerId, out IWeaponMuzzleSource source) || source == null)
            { muzzle = default; return false; }
            muzzle = source.CurrentMuzzle;
            return true;
        }
    }
}
