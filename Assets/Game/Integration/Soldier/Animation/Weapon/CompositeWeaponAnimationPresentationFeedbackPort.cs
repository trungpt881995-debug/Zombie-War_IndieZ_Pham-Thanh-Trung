using System;
using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;
using ZombieWar.Integration.Audio.Weapon;

namespace ZombieWar.Integration.Soldier.Animation.Weapon
{
    /// <summary>
    /// Outermost Weapon presentation composite.
    /// Existing VFX + game-feel + audio behavior is preserved exactly once,
    /// then Soldier animation is added as an independent presentation channel.
    /// </summary>
    public sealed class CompositeWeaponAnimationPresentationFeedbackPort :
        IWeaponFeedbackPort
    {
        private readonly CompositeWeaponPresentationFeedbackPort _existing;
        private readonly WeaponSoldierAnimationFeedbackPort _animation;

        public CompositeWeaponAnimationPresentationFeedbackPort(
            CompositeWeaponPresentationFeedbackPort existing,
            WeaponSoldierAnimationFeedbackPort animation)
        {
            _existing = existing ??
                throw new ArgumentNullException(nameof(existing));

            _animation = animation ??
                throw new ArgumentNullException(nameof(animation));
        }

        public void OnShotFired(
            EntityId ownerId,
            WeaponType weapon)
        {
            _existing.OnShotFired(
                ownerId,
                weapon);

            _animation.OnShotFired(
                ownerId,
                weapon);
        }

        public void OnFlameStarted(EntityId ownerId)
        {
            _existing.OnFlameStarted(ownerId);
            _animation.OnFlameStarted(ownerId);
        }

        public void OnFlameStopped(EntityId ownerId)
        {
            _existing.OnFlameStopped(ownerId);
            _animation.OnFlameStopped(ownerId);
        }
    }
}
