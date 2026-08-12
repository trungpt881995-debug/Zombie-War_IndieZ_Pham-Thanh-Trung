using System;
using ZombieWar.Features.Weapon.Catalog;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Model
{
    public sealed class WeaponLoadoutModel
    {
        private const int WeaponCount = 6;
        private readonly IWeaponCatalog _catalog;
        private readonly WeaponType _initialWeapon;
        private readonly float[] _cooldowns = new float[WeaponCount];

        public WeaponType CurrentWeapon { get; private set; }
        public bool GameplayEnabled { get; private set; } = true;

        public WeaponLoadoutModel(IWeaponCatalog catalog, WeaponType initialWeapon)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _catalog.Get(initialWeapon);
            _initialWeapon = initialWeapon;
            CurrentWeapon = initialWeapon;
        }

        public WeaponSelectionResult TrySelect(WeaponType requested)
        {
            if (!GameplayEnabled)
                return WeaponSelectionResult.Rejected(CurrentWeapon, WeaponSelectionRejectReason.GameplayDisabled);
            if (!_catalog.TryGet(requested, out _))
                return WeaponSelectionResult.Rejected(CurrentWeapon, WeaponSelectionRejectReason.UnknownWeapon);
            if (requested == CurrentWeapon)
                return WeaponSelectionResult.Rejected(CurrentWeapon, WeaponSelectionRejectReason.AlreadySelected);
            if (GetCooldownRemaining(requested) > 0f)
                return WeaponSelectionResult.Rejected(CurrentWeapon, WeaponSelectionRejectReason.OnCooldown);

            WeaponType previous = CurrentWeapon;
            WeaponDefinition previousDefinition = _catalog.Get(previous);
            _cooldowns[(int)previous] = previousDefinition.SelectionCooldown;
            CurrentWeapon = requested;
            return WeaponSelectionResult.Success(previous, CurrentWeapon);
        }

        public bool TickCooldown(WeaponType type, float deltaTime)
        {
            if (!GameplayEnabled || deltaTime <= 0f) return false;
            int index = (int)type;
            float previous = _cooldowns[index];
            if (previous <= 0f) return false;
            float next = previous - deltaTime;
            if (next <= 0f)
            {
                _cooldowns[index] = 0f;
                return true;
            }
            _cooldowns[index] = next;
            return false;
        }

        public float GetCooldownRemaining(WeaponType type)
        {
            int index = (int)type;
            if (index < 0 || index >= WeaponCount)
                throw new ArgumentOutOfRangeException(nameof(type));
            return _cooldowns[index];
        }

        public void SetGameplayEnabled(bool enabled) => GameplayEnabled = enabled;

        public void ResetForGameLevel()
        {
            for (int i = 0; i < _cooldowns.Length; i++) _cooldowns[i] = 0f;
            CurrentWeapon = _initialWeapon;
            GameplayEnabled = true;
        }

        public WeaponCooldownSnapshot SnapshotCooldowns() =>
            new WeaponCooldownSnapshot(
                _cooldowns[0], _cooldowns[1], _cooldowns[2],
                _cooldowns[3], _cooldowns[4], _cooldowns[5]);
    }
}
