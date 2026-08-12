using System;
using System.Collections.Generic;
using ZombieWar.Features.Weapon.Domain;

namespace ZombieWar.Features.Weapon.Catalog
{
    public sealed class WeaponCatalog : IWeaponCatalog
    {
        public const int RequiredWeaponCount = 6;
        private readonly WeaponDefinition[] _definitions =
            new WeaponDefinition[RequiredWeaponCount];
        private readonly bool[] _present = new bool[RequiredWeaponCount];

        public int Count => RequiredWeaponCount;

        public WeaponCatalog(IReadOnlyList<WeaponDefinition> definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            if (definitions.Count != RequiredWeaponCount)
                throw new ArgumentException($"Weapon catalog requires exactly {RequiredWeaponCount} definitions.", nameof(definitions));

            for (int i = 0; i < definitions.Count; i++)
            {
                WeaponDefinition definition = definitions[i];
                int index = ToIndex(definition.Type);
                if (_present[index])
                    throw new ArgumentException($"Duplicate WeaponType: {definition.Type}.", nameof(definitions));
                _definitions[index] = definition;
                _present[index] = true;
            }

            for (int i = 0; i < RequiredWeaponCount; i++)
                if (!_present[i])
                    throw new ArgumentException($"Missing WeaponType: {(WeaponType)i}.", nameof(definitions));
        }

        public WeaponDefinition Get(WeaponType type)
        {
            int index = ToIndex(type);
            if (!_present[index]) throw new InvalidOperationException($"Weapon {type} is not registered.");
            return _definitions[index];
        }

        public bool TryGet(WeaponType type, out WeaponDefinition definition)
        {
            int index;
            try { index = ToIndex(type); }
            catch (ArgumentOutOfRangeException) { definition = default; return false; }
            if (!_present[index]) { definition = default; return false; }
            definition = _definitions[index];
            return true;
        }

        private static int ToIndex(WeaponType type)
        {
            int index = (int)type;
            if (index < 0 || index >= RequiredWeaponCount)
                throw new ArgumentOutOfRangeException(nameof(type));
            return index;
        }
    }
}
