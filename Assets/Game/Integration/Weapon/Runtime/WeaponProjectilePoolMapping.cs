using System;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Projectile.Domain;

namespace ZombieWar.Integration.Weapon
{
    public readonly struct WeaponProjectilePoolMapping
    {
        public ProjectilePoolKey Pistol { get; }
        public ProjectilePoolKey AK { get; }
        public ProjectilePoolKey ShotgunPellet { get; }
        public ProjectilePoolKey Sniper { get; }
        public ProjectilePoolKey Grenade { get; }

        public WeaponProjectilePoolMapping(int pistol, int ak, int pellet, int sniper, int grenade)
        {
            Pistol = new ProjectilePoolKey(pistol);
            AK = new ProjectilePoolKey(ak);
            ShotgunPellet = new ProjectilePoolKey(pellet);
            Sniper = new ProjectilePoolKey(sniper);
            Grenade = new ProjectilePoolKey(grenade);
        }

        public ProjectilePoolKey Get(WeaponProjectileProfileId profile)
        {
            switch (profile)
            {
                case WeaponProjectileProfileId.PistolBullet: return Pistol;
                case WeaponProjectileProfileId.AKBullet: return AK;
                case WeaponProjectileProfileId.ShotgunPellet: return ShotgunPellet;
                case WeaponProjectileProfileId.SniperBullet: return Sniper;
                case WeaponProjectileProfileId.Grenade: return Grenade;
                default: throw new ArgumentOutOfRangeException(nameof(profile));
            }
        }

        public static WeaponProjectilePoolMapping Default =>
            new WeaponProjectilePoolMapping(1, 2, 3, 4, 5);
    }
}
