using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Model
{
    public sealed class WeaponFireSessionModel
    {
        public EntityId OwnerId { get; }
        public bool HasTarget { get; private set; }
        public EntityId TargetId { get; private set; }
        public WeaponType Weapon { get; private set; }
        public float TimeUntilNextFire { get; private set; }

        public bool Ready => HasTarget && TimeUntilNextFire <= 0f;

        public WeaponFireSessionModel(EntityId ownerId) => OwnerId = ownerId;

        public void Bind(WeaponType weapon, EntityId targetId)
        {
            Weapon = weapon;
            TargetId = targetId;
            HasTarget = true;
            TimeUntilNextFire = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (!HasTarget || deltaTime <= 0f || TimeUntilNextFire <= 0f) return;
            TimeUntilNextFire -= deltaTime;
            if (TimeUntilNextFire < 0f) TimeUntilNextFire = 0f;
        }

        public void ConsumeCadence(float interval)
        {
            TimeUntilNextFire = interval > 0f ? interval : 0f;
        }

        public void Clear()
        {
            HasTarget = false;
            TargetId = default;
            TimeUntilNextFire = 0f;
        }
    }
}
