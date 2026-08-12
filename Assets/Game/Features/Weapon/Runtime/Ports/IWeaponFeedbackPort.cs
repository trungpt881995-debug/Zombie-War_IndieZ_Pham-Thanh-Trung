using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Ports
{
    public interface IWeaponFeedbackPort
    {
        void OnShotFired(EntityId ownerId, WeaponType weapon);
        void OnFlameStarted(EntityId ownerId);
        void OnFlameStopped(EntityId ownerId);
    }
}
