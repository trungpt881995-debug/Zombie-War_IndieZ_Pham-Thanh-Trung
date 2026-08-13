using UnityEngine; using ZombieWar.Features.Spawn.Services; using ZombieWar.Features.Spawn.Unity.Runtime;
namespace ZombieWar.Features.Spawn.Unity.View
{
    public sealed class SpawnDebugView : MonoBehaviour
    {
        [SerializeField] private SpawnRuntimeRoot runtimeRoot; [SerializeField] private bool showOverlay;
        private void OnGUI(){if(!showOverlay||runtimeRoot==null)return;ISpawnRuntime r=runtimeRoot.Runtime;if(r==null)return;GUILayout.BeginArea(new Rect(10,10,320,210),GUI.skin.box);GUILayout.Label($"Spawn: {r.State}");GUILayout.Label($"Difficulty: {r.Difficulty}");GUILayout.Label($"Alive cap: {r.Tuning.MaxAlive}");GUILayout.Label($"Interval: {r.Elapsed:F2}/{r.Tuning.Interval:F2}");GUILayout.Label($"Batch: {r.Tuning.BatchMin}-{r.Tuning.BatchMax}");GUILayout.Label($"Last spawned: {r.LastBatch.Spawned}");GUILayout.Label($"Last reject: {r.LastBatch.LastRejectReason}");GUILayout.EndArea();}
    }
}
