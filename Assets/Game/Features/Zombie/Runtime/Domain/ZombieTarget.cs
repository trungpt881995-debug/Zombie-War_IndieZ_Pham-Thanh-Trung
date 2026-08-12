using GameplayCore.Entities;

namespace ZombieWar.Features.Zombie.Domain
{
    public readonly struct ZombieTarget
    {
        public static readonly ZombieTarget None = new ZombieTarget(false, default, default);
        public bool IsValid { get; }
        public EntityId EntityId { get; }
        public ZombiePoint Position { get; }

        private ZombieTarget(bool isValid, EntityId entityId, ZombiePoint position)
        {
            IsValid = isValid; EntityId = entityId; Position = position;
        }

        public static ZombieTarget From(EntityId entityId, in ZombiePoint position) =>
            new ZombieTarget(true, entityId, position);
    }
}
