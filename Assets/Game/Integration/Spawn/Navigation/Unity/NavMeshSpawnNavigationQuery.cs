using UnityEngine; using UnityEngine.AI; using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Ports;
namespace ZombieWar.Integration.Spawn.Navigation.Unity
{
    public sealed class NavMeshSpawnNavigationQuery : MonoBehaviour, ISpawnNavigationQuery
    {
        [SerializeField,Min(0.01f)] private float sampleDistance=4f; [SerializeField] private int areaMask=NavMesh.AllAreas;
        public bool TryResolve(in SpawnPoint candidate,out SpawnPoint resolved)
        {
            resolved=default;Vector3 source=new Vector3(candidate.X,candidate.Y,candidate.Z);if(!NavMesh.SamplePosition(source,out NavMeshHit hit,Mathf.Max(0.01f,sampleDistance),areaMask))return false;
            resolved=new SpawnPoint(hit.position.x,hit.position.y,hit.position.z);return true;
        }
    }
}
