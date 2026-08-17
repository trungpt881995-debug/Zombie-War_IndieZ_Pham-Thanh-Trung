using GameplayCore.Entities;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;

namespace ZombieWar.Features.Weapon.Strategies
{
    public sealed class ShotgunFireStrategy : IWeaponFireStrategy
    {
        private readonly IWeaponProjectilePort _projectiles;
        private readonly IWeaponFeedbackPort _feedback;

        public ShotgunFireStrategy(
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
                    out WeaponDirection baseDirection))
            {
                return false;
            }

            int pelletCount = weapon.ProjectileCount;
            float step = pelletCount > 1
                ? weapon.SpreadAngle / (pelletCount - 1)
                : 0f;
            float first = -weapon.SpreadAngle * 0.5f;

            int launchedCount = 0;

            for (int i = 0; i < pelletCount; i++)
            {
                WeaponDirection direction =
                    baseDirection.RotateYawDegrees(first + step * i);

                var request = new WeaponProjectileRequest(
                    context.OwnerId,
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

                if (_projectiles.TryLaunch(in request))
                {
                    launchedCount++;
                }
            }

            if (launchedCount > 0)
            {
                _feedback.OnShotFired(context.OwnerId, weapon.Type);
            }

            return launchedCount == pelletCount;
        }

        public void OnTargetCleared(EntityId ownerId)
        {
        }
    }
}
