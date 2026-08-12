using System;

namespace ZombieWar.Features.Camera.Domain
{
    public readonly struct CameraProfile : IEquatable<CameraProfile>
    {
        public CameraProjectionMode ProjectionMode { get; }
        public float FieldOfView { get; }
        public float OrthographicSize { get; }
        public float NearClip { get; }
        public float FarClip { get; }

        public float Pitch { get; }
        public float Yaw { get; }
        public float Roll { get; }

        public float OffsetX { get; }
        public float OffsetY { get; }
        public float OffsetZ { get; }

        public float DampingX { get; }
        public float DampingY { get; }
        public float DampingZ { get; }

        public CameraProfile(
            CameraProjectionMode projectionMode,
            float fieldOfView,
            float orthographicSize,
            float nearClip,
            float farClip,
            float pitch,
            float yaw,
            float roll,
            float offsetX,
            float offsetY,
            float offsetZ,
            float dampingX,
            float dampingY,
            float dampingZ)
        {
            ValidateFinite(fieldOfView, nameof(fieldOfView));
            ValidateFinite(orthographicSize, nameof(orthographicSize));
            ValidateFinite(nearClip, nameof(nearClip));
            ValidateFinite(farClip, nameof(farClip));
            ValidateFinite(pitch, nameof(pitch));
            ValidateFinite(yaw, nameof(yaw));
            ValidateFinite(roll, nameof(roll));
            ValidateFinite(offsetX, nameof(offsetX));
            ValidateFinite(offsetY, nameof(offsetY));
            ValidateFinite(offsetZ, nameof(offsetZ));
            ValidateFinite(dampingX, nameof(dampingX));
            ValidateFinite(dampingY, nameof(dampingY));
            ValidateFinite(dampingZ, nameof(dampingZ));

            if (fieldOfView <= 0f || fieldOfView >= 180f)
                throw new ArgumentOutOfRangeException(nameof(fieldOfView));
            if (orthographicSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(orthographicSize));
            if (nearClip <= 0f || farClip <= nearClip)
                throw new ArgumentOutOfRangeException(nameof(farClip));
            if (dampingX < 0f || dampingY < 0f || dampingZ < 0f)
                throw new ArgumentOutOfRangeException(nameof(dampingX));

            ProjectionMode = projectionMode;
            FieldOfView = fieldOfView;
            OrthographicSize = orthographicSize;
            NearClip = nearClip;
            FarClip = farClip;
            Pitch = pitch;
            Yaw = yaw;
            Roll = roll;
            OffsetX = offsetX;
            OffsetY = offsetY;
            OffsetZ = offsetZ;
            DampingX = dampingX;
            DampingY = dampingY;
            DampingZ = dampingZ;
        }

        public bool Equals(CameraProfile other) =>
            ProjectionMode == other.ProjectionMode &&
            FieldOfView.Equals(other.FieldOfView) &&
            OrthographicSize.Equals(other.OrthographicSize) &&
            NearClip.Equals(other.NearClip) && FarClip.Equals(other.FarClip) &&
            Pitch.Equals(other.Pitch) && Yaw.Equals(other.Yaw) && Roll.Equals(other.Roll) &&
            OffsetX.Equals(other.OffsetX) && OffsetY.Equals(other.OffsetY) && OffsetZ.Equals(other.OffsetZ) &&
            DampingX.Equals(other.DampingX) && DampingY.Equals(other.DampingY) && DampingZ.Equals(other.DampingZ);

        public override bool Equals(object obj) => obj is CameraProfile other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)ProjectionMode, FieldOfView, OrthographicSize, NearClip, FarClip, Pitch, Yaw, Roll);

        private static void ValidateFinite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
