using System;
namespace ZombieWar.Features.Spawn.Domain
{
    public readonly struct SpawnSector : IEquatable<SpawnSector>
    {
        public SpawnSectorId Id { get; }
        public SpawnArea Area { get; }
        public SpawnSector(SpawnSectorId id, in SpawnArea area)
        {
            if (!area.IsValid) throw new ArgumentException("Spawn sector area must be valid.", nameof(area));
            Id=id; Area=area;
        }
        public bool Equals(SpawnSector other) => Id==other.Id && Area.Equals(other.Area);
        public override bool Equals(object obj) => obj is SpawnSector other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Id,Area);
    }
}
