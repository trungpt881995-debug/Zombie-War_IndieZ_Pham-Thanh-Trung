using System;
using System.Collections.Generic;
using GameplayCore.Entities;
using ZombieWar.Features.Soldier.View;

namespace ZombieWar.Integration.Soldier.Animation
{
    public sealed class SoldierWeaponAnimationRegistry :
        ISoldierWeaponAnimationRegistry
    {
        private readonly Dictionary<EntityId, ISoldierWeaponAnimationView> _views =
            new Dictionary<EntityId, ISoldierWeaponAnimationView>(4);

        public void Register(
            EntityId ownerId,
            ISoldierWeaponAnimationView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            _views[ownerId] = view;
        }

        public void Unregister(EntityId ownerId)
        {
            _views.Remove(ownerId);
        }

        public bool TryGet(
            EntityId ownerId,
            out ISoldierWeaponAnimationView view)
        {
            return _views.TryGetValue(
                ownerId,
                out view);
        }

        public void Clear()
        {
            _views.Clear();
        }
    }
}
