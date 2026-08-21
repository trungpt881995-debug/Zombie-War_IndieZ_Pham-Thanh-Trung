using GameplayCore.Entities;

namespace ZombieWar.Features.Boss.Domain
{
    public readonly struct BossTarget
    {
        public static readonly BossTarget None = new BossTarget(false, default, default);
        public bool IsValid
        {
            get;
        }
        public EntityId EntityId
        {
            get;
        }
        public BossPoint Position
        {
            get;
        }
        private BossTarget(bool valid, EntityId id, BossPoint position)
        {
            IsValid = valid;
            EntityId = id;
            Position = position;
        }
        public static BossTarget From(EntityId id, in BossPoint p) => new BossTarget(true, id, p);
    }
}
