using System;

namespace ZombieWar.Features.Weapon.Domain
{
    public readonly struct WeaponCooldownSnapshot
    {
        private readonly float _pistol;
        private readonly float _ak;
        private readonly float _shotgun;
        private readonly float _sniper;
        private readonly float _grenade;
        private readonly float _flame;

        public WeaponCooldownSnapshot(
            float pistol, float ak, float shotgun,
            float sniper, float grenade, float flame)
        {
            _pistol = pistol; _ak = ak; _shotgun = shotgun;
            _sniper = sniper; _grenade = grenade; _flame = flame;
        }

        public float Get(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Pistol: return _pistol;
                case WeaponType.AK: return _ak;
                case WeaponType.Shotgun: return _shotgun;
                case WeaponType.SniperRifle: return _sniper;
                case WeaponType.GrenadeLauncher: return _grenade;
                case WeaponType.Flamethrower: return _flame;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
