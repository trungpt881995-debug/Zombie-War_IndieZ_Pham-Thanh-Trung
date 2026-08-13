using UnityEngine; using ZombieWar.Features.Level.Services; using ZombieWar.Features.Level.Unity.Runtime;
namespace ZombieWar.Features.Level.Unity.View
{
    public sealed class LevelDebugView:MonoBehaviour
    {
        [SerializeField] private LevelRuntimeRoot runtimeRoot; [SerializeField] private bool showOverlay=true;
        private void OnGUI(){if(!showOverlay||runtimeRoot==null||runtimeRoot.Runtime==null)return;ILevelRuntime r=runtimeRoot.Runtime;var s=r.Snapshot();GUILayout.BeginArea(new Rect(20,350,330,250),GUI.skin.box);GUILayout.Label("LEVEL DEBUG");GUILayout.Label($"Game Level: {s.GameLevel}");GUILayout.Label($"State: {s.State} / {s.Phase}");GUILayout.Label($"Soldier Group Level: {s.SoldierGroupLevel}");GUILayout.Label($"Normal Kills: {s.NormalZombieKillCount} / {s.NextThreshold}");GUILayout.Label($"Progression Enabled: {s.ProgressionEnabled}");GUILayout.Label($"Boss: {s.DefeatedBossObjectives} / {s.RequiredBossObjectives}");GUILayout.EndArea();}
    }
}
