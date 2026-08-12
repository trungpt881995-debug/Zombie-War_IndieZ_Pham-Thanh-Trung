using UnityEngine;
using ZombieWar.Features.Camera.Domain;
using ZombieWar.Features.Camera.Unity.Runtime;
using ZombieWar.Features.Soldier.View;

namespace ZombieWar.Integration.Camera.Soldier
{
    public sealed class SoldierGroupCameraTargetAdapter : CameraTargetProviderBehaviour
    {
        [SerializeField] private SoldierGroupView soldierGroupView;
        [SerializeField] private Transform cameraTarget;

        public override bool TryGetTarget(out CameraPoint position)
        {
            Transform source = cameraTarget;
            if (source == null && soldierGroupView != null) source = soldierGroupView.transform;
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
