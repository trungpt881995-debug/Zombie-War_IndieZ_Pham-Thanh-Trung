using UnityEngine;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Unity.Config
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Zombie War/Weapon/Weapon Config")]
    public sealed class WeaponConfig : ScriptableObject
    {
        [SerializeField] private WeaponType weaponType = WeaponType.Pistol;

        [Tooltip("Damage applied by each successful projectile/raycast hit. For Shotgun this is damage per pellet.")]
        [Min(0.0001f)]
        [SerializeField] private float damage = 10f;

        [Tooltip("Shots per second. Ignored by Flamethrower.")]
        [Min(0f)]
        [SerializeField] private float fireRate = 2.5f;

        [Tooltip("Projectile speed retained by the shared projectile contract. Hitscan weapons resolve immediately, so this value does not change ray travel time.")]
        [Min(0f)]
        [SerializeField] private float projectileSpeed = 20f;

        [Tooltip("Maximum raycast/projectile range for this weapon.")]
        [Min(0f)]
        [SerializeField] private float maxRange = 30f;

        [Tooltip("Maximum targeting distance used before the weapon is allowed to attack.")]
        [Min(0.0001f)]
        [SerializeField] private float targetRange = 15f;

        [Min(0f)]
        [SerializeField] private float selectionCooldown = 2f;

        [Tooltip("Total horizontal spread angle in degrees. Primarily used by Shotgun.")]
        [Min(0f)]
        [SerializeField] private float spreadAngle = 0f;

        [Tooltip("Number of projectile/raycast launches per accepted shot. Use 7 for Shotgun and 1 for Pistol/AK/Sniper/Grenade. Ignored by Flamethrower.")]
        [Min(1)]
        [SerializeField] private int projectileCount = 1;

        [Tooltip("Retained by the shared projectile contract. Hitscan weapons resolve immediately.")]
        [Min(0f)]
        [SerializeField] private float projectileLifetime = 3f;

        [Tooltip("Explosion radius for Grenade Launcher. Ignored by non-explosive weapons.")]
        [Min(0f)]
        [SerializeField] private float explosionRadius = 0f;

        [Min(0f)]
        [SerializeField] private float flameTickInterval = 0.1f;

        [Min(0f)]
        [SerializeField] private float flameRadius = 1.5f;

        public WeaponType WeaponType => weaponType;

        public WeaponDefinition CreateDefinition() =>
            new WeaponDefinition(
                weaponType,
                damage,
                fireRate,
                projectileSpeed,
                maxRange,
                targetRange,
                selectionCooldown,
                spreadAngle,
                projectileLifetime,
                explosionRadius,
                flameTickInterval,
                flameRadius,
                Mathf.Max(1, projectileCount));

        private void OnValidate()
        {
            if (projectileCount < 1)
            {
                projectileCount = 1;
            }
        }
    }
}
