using UnityEngine;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Unity.Authoring
{
    [DisallowMultipleComponent]
    public sealed class MapBossSpawnPoint : MonoBehaviour
    {
        public MapPoint Position
        {
            get
            {
                Vector3 p = transform.position;
                return new MapPoint(p.x, p.y, p.z);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, 0.75f);
        }
    }
}
