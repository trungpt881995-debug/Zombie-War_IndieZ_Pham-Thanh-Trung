using ZombieWar.Features.Camera.Domain;

namespace ZombieWar.Features.Camera.Model
{
    public sealed class CameraModel
    {
        public CameraState State { get; private set; } = CameraState.Uninitialized;
        public CameraProfile Profile { get; private set; }
        public bool HasTarget { get; private set; }
        public bool HasBounds { get; private set; }
        public CameraPoint RawTarget { get; private set; }
        public CameraPoint ConstrainedTarget { get; private set; }
        public CameraBounds Bounds { get; private set; }

        public void Initialize(in CameraProfile profile)
        {
            Profile = profile;
            State = CameraState.Ready;
            HasTarget = false;
            HasBounds = false;
        }

        public void SetGameplayEnabled(bool enabled)
        {
            if (State == CameraState.Uninitialized) return;
            State = enabled ? CameraState.Active : CameraState.Suspended;
        }

        public void SetProfile(in CameraProfile profile) => Profile = profile;

        public void SetTarget(in CameraPoint raw, in CameraPoint constrained)
        {
            RawTarget = raw;
            ConstrainedTarget = constrained;
            HasTarget = true;
        }

        public void ClearTarget() => HasTarget = false;

        public void SetBounds(in CameraBounds bounds)
        {
            Bounds = bounds;
            HasBounds = true;
        }

        public void ClearBounds() => HasBounds = false;

        public void Reset()
        {
            State = CameraState.Uninitialized;
            HasTarget = false;
            HasBounds = false;
            Profile = default;
            RawTarget = default;
            ConstrainedTarget = default;
            Bounds = default;
        }
    }
}
