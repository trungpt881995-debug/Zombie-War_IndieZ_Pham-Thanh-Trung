using UnityEngine;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Ports;

namespace ZombieWar.Features.Camera.Unity.Runtime
{
    public abstract class CameraTargetProviderBehaviour : MonoBehaviour, ICameraTargetProvider
    {
        public abstract bool TryGetTarget(out CameraPoint position);
    }
}
