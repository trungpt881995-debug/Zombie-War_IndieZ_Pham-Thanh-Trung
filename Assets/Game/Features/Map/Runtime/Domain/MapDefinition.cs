using System;

namespace ZombieWar.Features.Map.Domain
{
    public readonly struct MapDefinition : IEquatable<MapDefinition>
    {
        public MapId Id { get; }
        public string AssetKey { get; }

        public MapDefinition(MapId id, string assetKey)
        {
            if (id == MapId.None) throw new ArgumentOutOfRangeException(nameof(id));
            if (string.IsNullOrWhiteSpace(assetKey)) throw new ArgumentException("Asset key is required.", nameof(assetKey));
            Id = id;
            AssetKey = assetKey;
        }

        public bool Equals(MapDefinition other) => Id == other.Id && string.Equals(AssetKey, other.AssetKey, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is MapDefinition other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Id, AssetKey);
    }
}
