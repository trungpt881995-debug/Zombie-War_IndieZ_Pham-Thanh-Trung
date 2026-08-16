using System;
using UnityEngine;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Integration.Weapon.Unity
{
    [Serializable]
    public sealed class WeaponVisualEntry
    {
        [SerializeField]
        private WeaponType weaponType;

        [SerializeField]
        private GameObject visualRoot;

        [SerializeField]
        private Transform muzzle;

        public WeaponType WeaponType => weaponType;
        public GameObject VisualRoot => visualRoot;
        public Transform Muzzle => muzzle;

        public WeaponVisualEntry(WeaponType weaponType)
        {
            this.weaponType = weaponType;
        }
    }

    /// <summary>
    /// Scene presentation adapter for one Soldier's weapon assets.
    ///
    /// Responsibilities:
    /// - Own the six authored weapon visual references for one Soldier.
    /// - Show exactly one weapon visual at a time.
    /// - Expose the active weapon's muzzle through IWeaponMuzzleSource.
    ///
    /// It intentionally does not own weapon selection, cadence, targeting,
    /// damage, projectile launching, VFX, audio, or gameplay state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SoldierWeaponView : MonoBehaviour, IWeaponMuzzleSource
    {
        private const int WeaponCount = (int)WeaponType.Flamethrower + 1;

        [Header("Weapon Visuals")]
        [SerializeField]
        private WeaponVisualEntry[] weapons = new WeaponVisualEntry[WeaponCount];

        private readonly WeaponVisualEntry[] _byType =
            new WeaponVisualEntry[WeaponCount];

        private WeaponVisualEntry _current;
        private bool _lookupBuilt;

        public bool HasCurrentWeapon => _current != null;

        public WeaponType CurrentWeapon =>
            _current != null
                ? _current.WeaponType
                : WeaponType.Pistol;

        public WeaponMuzzle CurrentMuzzle
        {
            get
            {
                EnsureLookup();

                if (_current == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(SoldierWeaponView)} on '{name}' has no active weapon. " +
                        "WeaponRuntimeRoot must apply the current weapon before combat starts.");
                }

                Transform muzzle = _current.Muzzle;
                Vector3 position = muzzle.position;
                Vector3 forward = muzzle.forward;

                var point = new WeaponPoint(
                    position.x,
                    position.y,
                    position.z);

                var direction = new WeaponDirection(
                    forward.x,
                    forward.y,
                    forward.z);

                return new WeaponMuzzle(
                    in point,
                    in direction);
            }
        }

        /// <summary>
        /// Presentation-only switch. The Weapon Feature remains the source of truth
        /// for which weapon is selected; this view mirrors that state.
        /// </summary>
        public void ShowWeapon(WeaponType weaponType)
        {
            EnsureLookup();

            int index = ToIndex(weaponType);
            WeaponVisualEntry selected = _byType[index];

            for (int i = 0; i < weapons.Length; i++)
            {
                WeaponVisualEntry entry = weapons[i];
                bool shouldBeActive = ReferenceEquals(entry, selected);
                GameObject visualRoot = entry.VisualRoot;

                if (visualRoot.activeSelf != shouldBeActive)
                {
                    visualRoot.SetActive(shouldBeActive);
                }
            }

            _current = selected;
        }

        /// <summary>
        /// Fail-fast validation used by the scene composition edge.
        /// </summary>
        public void ValidateConfiguration()
        {
            RebuildLookup();
        }

        private void Reset()
        {
            weapons = new WeaponVisualEntry[WeaponCount];

            for (int i = 0; i < WeaponCount; i++)
            {
                weapons[i] = new WeaponVisualEntry((WeaponType)i);
            }

            _lookupBuilt = false;
            _current = null;
        }

        private void OnValidate()
        {
            _lookupBuilt = false;
            _current = null;
        }

        private void EnsureLookup()
        {
            if (!_lookupBuilt)
            {
                RebuildLookup();
            }
        }

        private void RebuildLookup()
        {
            Array.Clear(_byType, 0, _byType.Length);

            if (weapons == null || weapons.Length != WeaponCount)
            {
                throw new InvalidOperationException(
                    $"{nameof(SoldierWeaponView)} on '{name}' requires exactly " +
                    $"{WeaponCount} weapon entries.");
            }

            for (int i = 0; i < weapons.Length; i++)
            {
                WeaponVisualEntry entry = weapons[i];

                if (entry == null)
                {
                    throw new InvalidOperationException(
                        $"Weapon entry {i} on '{name}' is not configured.");
                }

                int index = ToIndex(entry.WeaponType);

                if (_byType[index] != null)
                {
                    throw new InvalidOperationException(
                        $"Duplicate WeaponType '{entry.WeaponType}' on '{name}'.");
                }

                if (entry.VisualRoot == null)
                {
                    throw new InvalidOperationException(
                        $"Visual Root for '{entry.WeaponType}' on '{name}' is not assigned.");
                }

                if (entry.Muzzle == null)
                {
                    throw new InvalidOperationException(
                        $"Muzzle for '{entry.WeaponType}' on '{name}' is not assigned.");
                }

                ValidateNoDuplicateReferences(entry, i);
                _byType[index] = entry;
            }

            for (int i = 0; i < WeaponCount; i++)
            {
                if (_byType[i] == null)
                {
                    throw new InvalidOperationException(
                        $"Missing WeaponType '{(WeaponType)i}' on '{name}'.");
                }
            }

            _lookupBuilt = true;
        }

        private void ValidateNoDuplicateReferences(
            WeaponVisualEntry current,
            int currentIndex)
        {
            for (int i = 0; i < currentIndex; i++)
            {
                WeaponVisualEntry previous = weapons[i];

                if (previous == null)
                {
                    continue;
                }

                if (previous.VisualRoot == current.VisualRoot)
                {
                    throw new InvalidOperationException(
                        $"Weapon visuals '{previous.WeaponType}' and '{current.WeaponType}' " +
                        $"on '{name}' reference the same Visual Root.");
                }

                if (previous.Muzzle == current.Muzzle)
                {
                    throw new InvalidOperationException(
                        $"Weapon visuals '{previous.WeaponType}' and '{current.WeaponType}' " +
                        $"on '{name}' reference the same Muzzle.");
                }
            }
        }

        private static int ToIndex(WeaponType weaponType)
        {
            int index = (int)weaponType;

            if (index < 0 || index >= WeaponCount)
            {
                throw new ArgumentOutOfRangeException(nameof(weaponType));
            }

            return index;
        }
    }
}
