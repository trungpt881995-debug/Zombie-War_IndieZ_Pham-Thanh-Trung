using UnityEngine;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Ports;

namespace ZombieWar.Features.Camera.Unity.Runtime
{
    public abstract class CameraRigBehaviour : MonoBehaviour, ICameraRig
    {
        public abstract bool IsReady { get; }
        public abstract void ApplyProfile(in CameraProfile profile);
        public abstract void SetTarget(in CameraPoint target);
        public abstract void SnapToTarget(in CameraPoint target);
        public abstract void SetEnabled(bool enabled);
    }
}
