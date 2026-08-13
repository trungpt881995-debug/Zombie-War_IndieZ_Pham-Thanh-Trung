using System;
namespace ZombieWar.Features.Spawn.Domain
{
    public readonly struct SpawnArea : IEquatable<SpawnArea>
    {
        public float MinX { get; }
        public float MaxX { get; }
        public float MinZ { get; }
        public float MaxZ { get; }
        public bool IsValid => MaxX > MinX && MaxZ > MinZ;
        public SpawnArea(float minX,float maxX,float minZ,float maxZ) { MinX=minX; MaxX=maxX; MinZ=minZ; MaxZ=maxZ; }
        public bool Contains(in SpawnPoint p) => p.X >= MinX && p.X <= MaxX && p.Z >= MinZ && p.Z <= MaxZ;
        public bool Equals(SpawnArea other) => MinX.Equals(other.MinX)&&MaxX.Equals(other.MaxX)&&MinZ.Equals(other.MinZ)&&MaxZ.Equals(other.MaxZ);
        public override bool Equals(object obj) => obj is SpawnArea other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(MinX,MaxX,MinZ,MaxZ);
    }
}
