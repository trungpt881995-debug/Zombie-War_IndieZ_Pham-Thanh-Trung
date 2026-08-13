using UnityEngine; using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Ports;
namespace ZombieWar.Integration.Spawn.Navigation.Unity
{
    public sealed class PassThroughSpawnNavigationQuery : MonoBehaviour, ISpawnNavigationQuery
    {
        public bool TryResolve(in SpawnPoint candidate,out SpawnPoint resolved){resolved=candidate;return true;}
    }
}
