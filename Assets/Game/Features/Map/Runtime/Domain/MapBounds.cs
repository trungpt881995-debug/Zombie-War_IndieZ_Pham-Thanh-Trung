using System;

namespace ZombieWar.Features.Map.Domain
{
    public readonly struct MapBounds : IEquatable<MapBounds>
    {
        public float MinX { get; }
        public float MaxX { get; }
        public float MinZ { get; }
        public float MaxZ { get; }

        public float Width => MaxX - MinX;
        public float Depth => MaxZ - MinZ;
        public bool IsValid => MaxX > MinX && MaxZ > MinZ;

        public MapBounds(float minX, float maxX, float minZ, float maxZ)
        {
            MinX = minX;
            MaxX = maxX;
            MinZ = minZ;
            MaxZ = maxZ;
        }

        public bool Contains(in MapPoint point)
        {
            return point.X >= MinX && point.X <= MaxX && point.Z >= MinZ && point.Z <= MaxZ;
        }

        public bool Equals(MapBounds other) =>
            MinX.Equals(other.MinX) && MaxX.Equals(other.MaxX) &&
            MinZ.Equals(other.MinZ) && MaxZ.Equals(other.MaxZ);

        public override bool Equals(object obj) => obj is MapBounds other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(MinX, MaxX, MinZ, MaxZ);
    }
}
