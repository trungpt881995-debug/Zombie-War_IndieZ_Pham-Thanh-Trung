using System;

namespace ZombieWar.Features.Camera.Domain
{
    public readonly struct CameraPoint : IEquatable<CameraPoint>
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public CameraPoint(float x, float y, float z)
        {
            if (!IsFinite(x) || !IsFinite(y) || !IsFinite(z))
                throw new ArgumentOutOfRangeException(nameof(x), "CameraPoint values must be finite.");
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(CameraPoint other) =>
            X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

        public override bool Equals(object obj) => obj is CameraPoint other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);
        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
