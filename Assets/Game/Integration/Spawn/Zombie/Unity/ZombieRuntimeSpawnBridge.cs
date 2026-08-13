using UnityEngine; using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Ports; using ZombieWar.Integration.Zombie.Unity;
namespace ZombieWar.Integration.Spawn.Zombie.Unity
{
    public sealed class ZombieRuntimeSpawnBridge : MonoBehaviour, IZombieSpawnPort, IZombiePopulationQuery
    {
        [SerializeField] private ZombieRuntimeRoot zombieRuntimeRoot;
        public int AliveCount=>zombieRuntimeRoot!=null?zombieRuntimeRoot.ActiveCount:0;
        public bool TrySpawn(in SpawnPoint position)
        {
            if(zombieRuntimeRoot==null||!zombieRuntimeRoot.IsInitialized)return false;
            return zombieRuntimeRoot.TrySpawn(new Vector3(position.X,position.Y,position.Z),out _);
        }
    }
}
