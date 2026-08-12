using UnityEngine;
using ZombieWar.Features.Zombie.Domain;

namespace ZombieWar.Integration.Zombie.Unity
{
    [DisallowMultipleComponent]
    public sealed class TransformZombieTargetSource : MonoBehaviour, IZombieTargetSource
    {
        public ZombiePoint Position
        {
            get
            {
                Vector3 p = transform.position;
                return new ZombiePoint(p.x, p.y, p.z);
            }
        }
        public bool IsActive => isActiveAndEnabled && gameObject.activeInHierarchy;
    }
}
