using System;

namespace ZombieWar.Features.Weapon.Domain
{
    public readonly struct WeaponDefinition
    {
        public WeaponType Type { get; }
        public float Damage { get; }
        public float FireRate { get; }
        public float ProjectileSpeed { get; }
        public float MaxRange { get; }
        public float TargetRange { get; }
        public float SelectionCooldown { get; }
        public float SpreadAngle { get; }
        public float ProjectileLifetime { get; }
        public float ExplosionRadius { get; }
        public float FlameTickInterval { get; }
        public float FlameRadius { get; }
        public WeaponProjectileProfileId ProjectileProfile { get; }

        public bool UsesProjectile =>
            Type != WeaponType.Flamethrower;

        public float FireInterval =>
            Type == WeaponType.Flamethrower
                ? FlameTickInterval
                : 1f / FireRate;

        public WeaponDefinition(
            WeaponType type,
            float damage,
            float fireRate,
            float projectileSpeed,
            float maxRange,
            float targetRange,
            float selectionCooldown,
            float spreadAngle,
            float projectileLifetime,
            float explosionRadius,
            float flameTickInterval,
            float flameRadius)
        {
            ValidateEnum(type);
            ValidatePositive(damage, nameof(damage));
            ValidatePositive(targetRange, nameof(targetRange));
            ValidateNonNegative(selectionCooldown, nameof(selectionCooldown));
            ValidateNonNegative(spreadAngle, nameof(spreadAngle));

            bool flame = type == WeaponType.Flamethrower;
            if (flame)
            {
                ValidatePositive(flameTickInterval, nameof(flameTickInterval));
                ValidatePositive(flameRadius, nameof(flameRadius));
                ValidateNonNegative(fireRate, nameof(fireRate));
                ValidateNonNegative(projectileSpeed, nameof(projectileSpeed));
                ValidateNonNegative(maxRange, nameof(maxRange));
                ValidateNonNegative(projectileLifetime, nameof(projectileLifetime));
                ValidateNonNegative(explosionRadius, nameof(explosionRadius));
            }
            else
            {
                ValidatePositive(fireRate, nameof(fireRate));
                ValidatePositive(projectileSpeed, nameof(projectileSpeed));
                ValidatePositive(maxRange, nameof(maxRange));
                ValidatePositive(projectileLifetime, nameof(projectileLifetime));
                if (type == WeaponType.GrenadeLauncher)
                    ValidatePositive(explosionRadius, nameof(explosionRadius));
                else
                    ValidateNonNegative(explosionRadius, nameof(explosionRadius));
                ValidateNonNegative(flameTickInterval, nameof(flameTickInterval));
                ValidateNonNegative(flameRadius, nameof(flameRadius));
            }

            Type = type;
            Damage = damage;
            FireRate = fireRate;
            ProjectileSpeed = projectileSpeed;
            MaxRange = maxRange;
            TargetRange = targetRange;
            SelectionCooldown = selectionCooldown;
            SpreadAngle = spreadAngle;
            ProjectileLifetime = projectileLifetime;
            ExplosionRadius = explosionRadius;
            FlameTickInterval = flameTickInterval;
            FlameRadius = flameRadius;
            ProjectileProfile = ResolveProfile(type);
        }

        private static WeaponProjectileProfileId ResolveProfile(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Pistol: return WeaponProjectileProfileId.PistolBullet;
                case WeaponType.AK: return WeaponProjectileProfileId.AKBullet;
                case WeaponType.Shotgun: return WeaponProjectileProfileId.ShotgunPellet;
                case WeaponType.SniperRifle: return WeaponProjectileProfileId.SniperBullet;
                case WeaponType.GrenadeLauncher: return WeaponProjectileProfileId.Grenade;
                case WeaponType.Flamethrower: return WeaponProjectileProfileId.None;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private static void ValidateEnum(WeaponType type)
        {
            int i = (int)type;
            if (i < 0 || i > (int)WeaponType.Flamethrower)
                throw new ArgumentOutOfRangeException(nameof(type));
        }
        private static void ValidatePositive(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
                throw new ArgumentOutOfRangeException(name);
        }
        private static void ValidateNonNegative(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
