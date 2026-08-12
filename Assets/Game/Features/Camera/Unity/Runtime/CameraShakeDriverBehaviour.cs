using UnityEngine;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Ports;

namespace ZombieWar.Features.Camera.Unity.Runtime
{
    public abstract class CameraShakeDriverBehaviour : MonoBehaviour, ICameraShakeDriver
    {
        public abstract bool TryPlay(in CameraShakeRequest request);
        public abstract void StopAll();
    }
}
