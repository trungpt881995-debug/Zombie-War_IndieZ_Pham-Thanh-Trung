using System;

namespace ZombieWar.Features.Camera.Domain
{
    public readonly struct CameraBounds : IEquatable<CameraBounds>
    {
        public float MinX { get; }
        public float MaxX { get; }
        public float MinZ { get; }
        public float MaxZ { get; }

        public bool IsValid =>
            IsFinite(MinX) && IsFinite(MaxX) &&
            IsFinite(MinZ) && IsFinite(MaxZ) &&
            MaxX > MinX && MaxZ > MinZ;

        public CameraBounds(float minX, float maxX, float minZ, float maxZ)
        {
            MinX = minX;
            MaxX = maxX;
            MinZ = minZ;
            MaxZ = maxZ;
        }

        public bool Contains(in CameraPoint point) =>
            point.X >= MinX && point.X <= MaxX &&
            point.Z >= MinZ && point.Z <= MaxZ;

        public CameraPoint Clamp(in CameraPoint point)
        {
            if (!IsValid) return point;
            float x = point.X < MinX ? MinX : (point.X > MaxX ? MaxX : point.X);
            float z = point.Z < MinZ ? MinZ : (point.Z > MaxZ ? MaxZ : point.Z);
            return new CameraPoint(x, point.Y, z);
        }

        public bool Equals(CameraBounds other) =>
            MinX.Equals(other.MinX) && MaxX.Equals(other.MaxX) &&
            MinZ.Equals(other.MinZ) && MaxZ.Equals(other.MaxZ);

        public override bool Equals(object obj) => obj is CameraBounds other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(MinX, MaxX, MinZ, MaxZ);

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
