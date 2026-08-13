using UnityEngine; using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Ports;
namespace ZombieWar.Integration.Spawn.Camera.Unity
{
    public sealed class UnityCameraSpawnVisibilityQuery : MonoBehaviour, ISpawnVisibilityQuery
    {
        [SerializeField] private UnityEngine.Camera gameplayCamera; [SerializeField,Range(0f,0.5f)] private float viewportPadding=0.05f;
        public bool IsVisible(in SpawnPoint point)
        {
            if(gameplayCamera==null||!gameplayCamera.isActiveAndEnabled)return true;
            Vector3 v=gameplayCamera.WorldToViewportPoint(new Vector3(point.X,point.Y,point.Z)); if(v.z<=0f)return false;
            float p=Mathf.Max(0f,viewportPadding);return v.x>=-p&&v.x<=1f+p&&v.y>=-p&&v.y<=1f+p;
        }
    }
}
