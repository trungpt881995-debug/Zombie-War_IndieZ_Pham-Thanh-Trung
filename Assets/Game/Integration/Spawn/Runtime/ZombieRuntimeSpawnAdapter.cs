using UnityEngine;
using ZombieWar.Features.Spawn.Domain;
using ZombieWar.Features.Spawn.Ports;
using ZombieWar.Integration.Zombie.Unity;

namespace ZombieWar.Integration.Spawn.Runtime
{
    /// <summary>
    /// Scene-edge adapter between Spawn Feature and Zombie Runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZombieRuntimeSpawnAdapter :
        MonoBehaviour,
        IZombieSpawnPort,
        IZombiePopulationQuery
    {
        [SerializeField]
        private ZombieRuntimeRoot zombieRuntimeRoot;

        public int AliveCount =>
            zombieRuntimeRoot != null
                ? zombieRuntimeRoot.ActiveCount
                : 0;

        public bool TrySpawn(
            in SpawnPoint position)
        {
            if (zombieRuntimeRoot == null ||
                !zombieRuntimeRoot.IsInitialized)
            {
                return false;
            }

            Vector3 worldPosition = new Vector3(
                position.X,
                position.Y,
                position.Z);

            return zombieRuntimeRoot.TrySpawn(
                worldPosition,
                out _);
        }
    }
}
