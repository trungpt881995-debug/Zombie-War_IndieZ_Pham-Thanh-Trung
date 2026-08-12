using UnityEngine;
using ZombieWar.Features.Map.Domain;

namespace ZombieWar.Features.Map.Unity.Authoring
{
    [DisallowMultipleComponent]
    public sealed class MapSpawnSectorVolume : MonoBehaviour
    {
        [SerializeField] private MapSpawnSectorId sectorId;
        [SerializeField] private Vector3 localCenter = Vector3.zero;
        [SerializeField] private Vector3 localSize = new Vector3(8f, 1f, 8f);
        [SerializeField] private bool drawGizmo = true;

        public MapSpawnSectorId SectorId => sectorId;

        public MapSpawnSector BuildSector()
        {
            Vector3 half = localSize * 0.5f;
            Vector3 c0 = transform.TransformPoint(localCenter + new Vector3(-half.x, 0f, -half.z));
            Vector3 c1 = transform.TransformPoint(localCenter + new Vector3(-half.x, 0f, half.z));
            Vector3 c2 = transform.TransformPoint(localCenter + new Vector3(half.x, 0f, -half.z));
            Vector3 c3 = transform.TransformPoint(localCenter + new Vector3(half.x, 0f, half.z));

            float minX = Mathf.Min(c0.x, c1.x, c2.x, c3.x);
            float maxX = Mathf.Max(c0.x, c1.x, c2.x, c3.x);
            float minZ = Mathf.Min(c0.z, c1.z, c2.z, c3.z);
            float maxZ = Mathf.Max(c0.z, c1.z, c2.z, c3.z);
            var area = new MapArea(minX, maxX, minZ, maxZ);
            return new MapSpawnSector(sectorId, in area);
        }

        private void OnValidate()
        {
            localSize.x = Mathf.Max(0.01f, localSize.x);
            localSize.y = Mathf.Max(0.01f, localSize.y);
            localSize.z = Mathf.Max(0.01f, localSize.z);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmo) return;
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(localCenter, localSize);
            Gizmos.matrix = old;
        }
    }
}
