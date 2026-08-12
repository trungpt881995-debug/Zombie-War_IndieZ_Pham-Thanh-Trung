using GameplayCore.Entities;

namespace ZombieWar.Features.Weapon.Domain
{
    public readonly struct WeaponFlameRequest
    {
        public EntityId OwnerId { get; }
        public EntityId TargetId { get; }
        public WeaponPoint Origin { get; }
        public WeaponDirection Direction { get; }
        public float Range { get; }
        public float Radius { get; }
        public float DamagePerTick { get; }

        public WeaponFlameRequest(
            EntityId ownerId,
            EntityId targetId,
            in WeaponPoint origin,
            in WeaponDirection direction,
            float range,
            float radius,
            float damagePerTick)
        {
            OwnerId = ownerId;
            TargetId = targetId;
            Origin = origin;
            Direction = direction;
            Range = range;
            Radius = radius;
            DamagePerTick = damagePerTick;
        }
    }
}
