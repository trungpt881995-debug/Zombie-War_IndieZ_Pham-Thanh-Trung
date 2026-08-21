using System;
using GameplayCore.Entities;
using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Services;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;

namespace ZombieWar.Integration.Weapon
{
    public sealed class WeaponToProjectileAdapter :
        IWeaponProjectilePort,
        IWeaponProjectileBinding
    {
        private IProjectileLauncher _launcher;

        public bool IsBound => _launcher != null;

        public void Bind(IProjectileLauncher launcher)
        {
            _launcher = launcher ??
                throw new ArgumentNullException(nameof(launcher));
        }

        public void Unbind()
        {
            _launcher = null;
        }

        public bool TryLaunch(in WeaponProjectileRequest request)
        {
            if (_launcher == null)
            {
                return false;
            }

            ProjectileImpactMode impactMode =
                ResolveImpactMode(request.Profile);

            WeaponPoint sourceOrigin = request.Origin;
            WeaponDirection sourceDirection = request.Direction;
            WeaponPoint sourceTarget = request.TargetPoint;

            var origin = new ProjectilePoint(
                sourceOrigin.X,
                sourceOrigin.Y,
                sourceOrigin.Z);

            var direction = new ProjectileDirection(
                sourceDirection.X,
                sourceDirection.Y,
                sourceDirection.Z);

            var target = new ProjectilePoint(
                sourceTarget.X,
                sourceTarget.Y,
                sourceTarget.Z);

            ProjectileLaunchRequest projectileRequest;

            if (request.HasTargetEntity)
            {
                projectileRequest = new ProjectileLaunchRequest(
                    request.OwnerId,
                    request.TargetId,
                    impactMode,
                    in origin,
                    in direction,
                    request.Damage,
                    request.MaxRange,
                    in target,
                    request.HasTargetPoint,
                    request.ExplosionRadius);
            }
            else
            {
                projectileRequest = new ProjectileLaunchRequest(
                    request.OwnerId,
                    impactMode,
                    in origin,
                    in direction,
                    request.Damage,
                    request.MaxRange,
                    in target,
                    request.HasTargetPoint,
                    request.ExplosionRadius);
            }

            return _launcher.TryLaunch(
                in projectileRequest,
                out EntityId _);
        }

        private static ProjectileImpactMode ResolveImpactMode(
            WeaponProjectileProfileId profile)
        {
            switch (profile)
            {
                case WeaponProjectileProfileId.SniperBullet:
                    return ProjectileImpactMode.Pierce;

                case WeaponProjectileProfileId.Grenade:
                    return ProjectileImpactMode.ExplodeOnGround;

                default:
                    return ProjectileImpactMode.StopOnHit;
            }
        }
    }
}
