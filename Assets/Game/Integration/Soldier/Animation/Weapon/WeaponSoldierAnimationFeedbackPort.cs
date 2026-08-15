using System;
using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;
using ZombieWar.Features.Soldier.View;

namespace ZombieWar.Integration.Soldier.Animation.Weapon
{
    /// <summary>
    /// Converts the existing Weapon "shot actually fired" presentation fact
    /// into an upper-body Soldier animation trigger.
    /// </summary>
    public sealed class WeaponSoldierAnimationFeedbackPort :
        IWeaponFeedbackPort
    {
        private readonly ISoldierWeaponAnimationRegistry _registry;

        public WeaponSoldierAnimationFeedbackPort(
            ISoldierWeaponAnimationRegistry registry)
        {
            _registry = registry ??
                throw new ArgumentNullException(nameof(registry));
        }

        public void OnShotFired(
            EntityId ownerId,
            WeaponType weapon)
        {
            _ = weapon;

            if (_registry.TryGet(
                    ownerId,
                    out ISoldierWeaponAnimationView view))
            {
                view.PlayShoot();
            }
        }

        public void OnFlameStarted(EntityId ownerId)
        {
            // Flamethrower uses continuous-fire semantics and intentionally does
            // not retrigger the one-shot Shoot state on every damage tick.
            _ = ownerId;
        }

        public void OnFlameStopped(EntityId ownerId)
        {
            _ = ownerId;
        }
    }
}
