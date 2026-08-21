using UnityEngine;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Unity.Authoring
{
    [DisallowMultipleComponent]
    public sealed class MapSoldierSpawnPoint : MonoBehaviour
    {
        public MapPoint Position
        {
            get
            {
                Vector3 position = transform.position;
                return new MapPoint(position.x, position.y, position.z);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, 0.75f);
        }
    }
}
