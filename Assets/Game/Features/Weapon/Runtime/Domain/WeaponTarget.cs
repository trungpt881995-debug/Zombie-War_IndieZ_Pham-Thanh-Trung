using GameplayCore.Entities;

namespace ZombieWar.Features.Weapon.Domain
{
    public readonly struct WeaponTarget
    {
        public EntityId TargetId { get; }
        public WeaponPoint Position { get; }

        public WeaponTarget(
            EntityId targetId,
            in WeaponPoint position)
        {
            TargetId = targetId;
            Position = position;
        }
    }
}
