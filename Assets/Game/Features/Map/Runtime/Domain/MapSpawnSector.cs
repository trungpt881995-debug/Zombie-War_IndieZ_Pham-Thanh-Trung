using System;

namespace ZombieWar.Features.Map.Domain
{
    public readonly struct MapSpawnSector : IEquatable<MapSpawnSector>
    {
        public MapSpawnSectorId Id { get; }
        public MapArea Area { get; }

        public MapSpawnSector(MapSpawnSectorId id, in MapArea area)
        {
            if (!area.IsValid) throw new ArgumentException("Spawn sector area must be valid.", nameof(area));
            Id = id;
            Area = area;
        }

        public bool Equals(MapSpawnSector other) => Id == other.Id && Area.Equals(other.Area);
        public override bool Equals(object obj) => obj is MapSpawnSector other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Id, Area);
    }
}
