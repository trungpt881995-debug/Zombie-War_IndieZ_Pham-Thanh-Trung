using GameplayCore.Entities;

namespace ZombieWar.Features.Soldier.Domain
{
    public readonly struct SoldierTargetInfo
    {
        public static readonly SoldierTargetInfo None = new SoldierTargetInfo(false, default, default);

        public bool HasTarget { get; }
        public EntityId TargetId { get; }
        public SoldierPoint Position { get; }

        private SoldierTargetInfo(bool hasTarget, EntityId targetId, SoldierPoint position)
        {
            HasTarget = hasTarget;
            TargetId = targetId;
            Position = position;
        }

        public static SoldierTargetInfo From(EntityId targetId,in SoldierPoint position)
        {
            return new SoldierTargetInfo(true,targetId,position);
        }
    }
}
