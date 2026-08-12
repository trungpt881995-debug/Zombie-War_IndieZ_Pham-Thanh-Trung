using UnityEngine;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Unity.Runtime;

namespace ZombieWar.Features.Camera.Unity.Target
{
    public sealed class TransformCameraTargetSource : CameraTargetProviderBehaviour
    {
        [SerializeField] private Transform target;

        public override bool TryGetTarget(out CameraPoint position)
        {
            Transform source = target != null ? target : transform;
            if (source == null || !source.gameObject.activeInHierarchy)
            {
                position = default;
                return false;
            }
            Vector3 p = source.position;
            position = new CameraPoint(p.x, p.y, p.z);
            return true;
        }
    }
}
