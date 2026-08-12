using UnityEngine;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Unity.Config
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Zombie War/Weapon/Weapon Config")]
    public sealed class WeaponConfig : ScriptableObject
    {
        [SerializeField] private WeaponType weaponType = WeaponType.Pistol;
        [Min(0.0001f)] [SerializeField] private float damage = 10f;
        [Tooltip("Shots per second. Ignored by Flamethrower.")]
        [Min(0f)] [SerializeField] private float fireRate = 2.5f;
        [Min(0f)] [SerializeField] private float projectileSpeed = 20f;
        [Min(0f)] [SerializeField] private float maxRange = 30f;
        [Min(0.0001f)] [SerializeField] private float targetRange = 15f;
        [Min(0f)] [SerializeField] private float selectionCooldown = 2f;
        [Min(0f)] [SerializeField] private float spreadAngle = 0f;
        [Min(0f)] [SerializeField] private float projectileLifetime = 3f;
        [Min(0f)] [SerializeField] private float explosionRadius = 0f;
        [Min(0f)] [SerializeField] private float flameTickInterval = 0.1f;
        [Min(0f)] [SerializeField] private float flameRadius = 1.5f;

        public WeaponType WeaponType => weaponType;

        public WeaponDefinition CreateDefinition() =>
            new WeaponDefinition(
                weaponType, damage, fireRate, projectileSpeed,
                maxRange, targetRange, selectionCooldown,
                spreadAngle, projectileLifetime, explosionRadius,
                flameTickInterval, flameRadius);
    }
}
