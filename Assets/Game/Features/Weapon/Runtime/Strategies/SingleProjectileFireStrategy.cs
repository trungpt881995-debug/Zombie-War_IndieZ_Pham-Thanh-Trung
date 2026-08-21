using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;

namespace ZombieWar.Features.Weapon.Strategies
{
    public sealed class SingleProjectileFireStrategy : IWeaponFireStrategy
    {
        private readonly IWeaponProjectilePort _projectiles;
        private readonly IWeaponFeedbackPort _feedback;

        public SingleProjectileFireStrategy(
            IWeaponProjectilePort projectiles,
            IWeaponFeedbackPort feedback)
        {
            _projectiles = projectiles;
            _feedback = feedback;
        }

        public void OnTargetAcquired(
            in WeaponDefinition weapon,
            in WeaponFireContext context)
        {
        }

        public bool Fire(
            in WeaponDefinition weapon,
            in WeaponFireContext context)
        {
            WeaponPoint origin = context.Muzzle.Position;
            WeaponPoint targetPoint = context.Target.Position;

            if (!WeaponDirection.TryFromTo(
                    in origin,
                    in targetPoint,
                    out WeaponDirection direction))
            {
                return false;
            }

            var request = new WeaponProjectileRequest(
                context.OwnerId,
                context.Target.TargetId,
                weapon.ProjectileProfile,
                in origin,
                in direction,
                weapon.ProjectileSpeed,
                weapon.Damage,
                weapon.MaxRange,
                weapon.ProjectileLifetime,
                in targetPoint,
                false,
                0f);

            bool launched = _projectiles.TryLaunch(in request);

            if (launched)
            {
                _feedback.OnShotFired(context.OwnerId, weapon.Type);
            }

            return launched;
        }

        public void OnTargetCleared(EntityId ownerId)
        {
        }
    }
}
