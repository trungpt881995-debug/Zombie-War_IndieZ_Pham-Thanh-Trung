using System;

namespace ZombieWar.Features.Map.Domain
{
    public readonly struct MapArea : IEquatable<MapArea>
    {
        public float MinX { get; }
        public float MaxX { get; }
        public float MinZ { get; }
        public float MaxZ { get; }

        public bool IsValid => MaxX > MinX && MaxZ > MinZ;
        public float Width => MaxX - MinX;
        public float Depth => MaxZ - MinZ;

        public MapArea(float minX, float maxX, float minZ, float maxZ)
        {
            MinX = minX;
            MaxX = maxX;
            MinZ = minZ;
            MaxZ = maxZ;
        }

        public bool Contains(in MapPoint point) =>
            point.X >= MinX && point.X <= MaxX && point.Z >= MinZ && point.Z <= MaxZ;

        public bool Equals(MapArea other) =>
            MinX.Equals(other.MinX) && MaxX.Equals(other.MaxX) &&
            MinZ.Equals(other.MinZ) && MaxZ.Equals(other.MaxZ);

        public override bool Equals(object obj) => obj is MapArea other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(MinX, MaxX, MinZ, MaxZ);
    }
}
