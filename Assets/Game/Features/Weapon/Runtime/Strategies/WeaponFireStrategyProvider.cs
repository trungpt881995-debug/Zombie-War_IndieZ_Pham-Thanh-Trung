using System;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Ports;

namespace ZombieWar.Features.Weapon.Strategies
{
    public sealed class WeaponFireStrategyProvider : IWeaponFireStrategyProvider
    {
        private readonly IWeaponFireStrategy _single;
        private readonly IWeaponFireStrategy _shotgun;
        private readonly IWeaponFireStrategy _grenade;
        private readonly IWeaponFireStrategy _flame;

        public WeaponFireStrategyProvider(
            IWeaponProjectilePort projectiles,
            IWeaponFlamePort flame,
            IWeaponFeedbackPort feedback)
        {
            if (projectiles == null) throw new ArgumentNullException(nameof(projectiles));
            if (flame == null) throw new ArgumentNullException(nameof(flame));
            if (feedback == null) throw new ArgumentNullException(nameof(feedback));
            _single = new SingleProjectileFireStrategy(projectiles, feedback);
            _shotgun = new ShotgunFireStrategy(projectiles, feedback);
            _grenade = new GrenadeFireStrategy(projectiles, feedback);
            _flame = new FlamethrowerFireStrategy(flame, feedback);
        }

        public IWeaponFireStrategy Get(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Pistol:
                case WeaponType.AK:
                case WeaponType.SniperRifle:
                    return _single;
                case WeaponType.Shotgun:
                    return _shotgun;
                case WeaponType.GrenadeLauncher:
                    return _grenade;
                case WeaponType.Flamethrower:
                    return _flame;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
