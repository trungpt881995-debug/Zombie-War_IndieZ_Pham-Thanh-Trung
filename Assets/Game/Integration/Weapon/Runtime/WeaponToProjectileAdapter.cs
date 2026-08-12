using System;
using GameplayCore.Entities;
using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Services;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;

namespace ZombieWar.Integration.Weapon
{
    public sealed class WeaponToProjectileAdapter : IWeaponProjectilePort, IWeaponProjectileBinding
    {
        private IProjectileLauncher _launcher;
        private WeaponProjectilePoolMapping _mapping = WeaponProjectilePoolMapping.Default;
        public bool IsBound => _launcher != null;

        public void Bind(IProjectileLauncher launcher, in WeaponProjectilePoolMapping mapping)
        {
            _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
            _mapping = mapping;
        }

        public void Unbind() => _launcher = null;

        public bool TryLaunch(in WeaponProjectileRequest request)
        {
            if (_launcher == null) return false;
            ProjectilePoolKey poolKey = _mapping.Get(request.Profile);
            ProjectileMotionKind motion = request.Profile == WeaponProjectileProfileId.Grenade
                ? ProjectileMotionKind.Ballistic : ProjectileMotionKind.Linear;
            ProjectileImpactMode impact;
            switch (request.Profile)
            {
                case WeaponProjectileProfileId.SniperBullet:
                    impact = ProjectileImpactMode.Pierce;
                    break;
                case WeaponProjectileProfileId.Grenade:
                    impact = ProjectileImpactMode.ExplodeOnGround;
                    break;
                default:
                    impact = ProjectileImpactMode.StopOnHit;
                    break;
            }

            WeaponPoint sourceOrigin = request.Origin;
            WeaponDirection sourceDirection = request.Direction;
            WeaponPoint sourceTarget = request.TargetPoint;
            var origin = new ProjectilePoint(sourceOrigin.X, sourceOrigin.Y, sourceOrigin.Z);
            var direction = new ProjectileDirection(sourceDirection.X, sourceDirection.Y, sourceDirection.Z);
            var target = new ProjectilePoint(sourceTarget.X, sourceTarget.Y, sourceTarget.Z);
            var projectileRequest = new ProjectileLaunchRequest(
                request.OwnerId, poolKey, motion, impact,
                in origin, in direction,
                request.Speed, request.Damage, request.MaxRange, request.MaxLifetime,
                in target, request.HasTargetPoint, request.ExplosionRadius);
            return _launcher.TryLaunch(in projectileRequest, out EntityId _);
        }
    }
}
