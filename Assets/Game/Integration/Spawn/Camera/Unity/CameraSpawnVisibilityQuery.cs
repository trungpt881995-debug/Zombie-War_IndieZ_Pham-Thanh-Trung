using UnityEngine;
using ZombieWar.Features.Spawn.Domain;
using ZombieWar.Features.Spawn.Ports;

namespace ZombieWar.Integration.Spawn.Visibility.Unity
{
    /// <summary>
    /// Camera-frustum visibility query for Spawn placement validation.
    /// true means visible/near viewport; the Spawn validator rejects it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraSpawnVisibilityQuery :
        MonoBehaviour,
        ISpawnVisibilityQuery
    {
        [SerializeField]
        private Camera gameplayCamera;

        [SerializeField, Min(0f)]
        private float viewportGuard = 0.05f;

        public bool IsVisible(
            in SpawnPoint point)
        {
            if (gameplayCamera == null)
            {
                // Fail closed: reject spawning until the camera is assigned.
                return true;
            }

            Vector3 worldPosition = new Vector3(
                point.X,
                point.Y,
                point.Z);

            Vector3 viewport =
                gameplayCamera.WorldToViewportPoint(worldPosition);

            if (viewport.z <= 0f)
            {
                return false;
            }

            float guard = Mathf.Max(0f, viewportGuard);

            return viewport.x >= -guard &&
                   viewport.x <= 1f + guard &&
                   viewport.y >= -guard &&
                   viewport.y <= 1f + guard;
        }
    }
}
