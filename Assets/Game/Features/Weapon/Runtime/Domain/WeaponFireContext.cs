using GameplayCore.Entities;

namespace ZombieWar.Features.Weapon.Domain
{
    public readonly struct WeaponFireContext
    {
        public EntityId OwnerId { get; }
        public WeaponMuzzle Muzzle { get; }
        public WeaponTarget Target { get; }

        public WeaponFireContext(
            EntityId ownerId,
            in WeaponMuzzle muzzle,
            in WeaponTarget target)
        {
            OwnerId = ownerId;
            Muzzle = muzzle;
            Target = target;
        }
    }
}
