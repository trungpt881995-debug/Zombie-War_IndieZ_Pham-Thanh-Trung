using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Ports
{
    public sealed class NullWeaponFeedbackPort : IWeaponFeedbackPort
    {
        public static readonly NullWeaponFeedbackPort Instance = new NullWeaponFeedbackPort();
        private NullWeaponFeedbackPort() { }
        public void OnShotFired(EntityId ownerId, WeaponType weapon) { }
        public void OnFlameStarted(EntityId ownerId) { }
        public void OnFlameStopped(EntityId ownerId) { }
    }
}
