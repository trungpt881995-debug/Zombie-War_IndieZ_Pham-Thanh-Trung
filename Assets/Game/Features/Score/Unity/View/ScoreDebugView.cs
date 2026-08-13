using UnityEngine;
using ZombieWar.Features.Score.Domain;
using ZombieWar.Features.Score.Unity.Runtime;

namespace ZombieWar.Features.Score.Unity.View
{
    public sealed class ScoreDebugView : MonoBehaviour
    {
        [SerializeField] private ScoreRuntimeRoot runtimeRoot;
        [SerializeField] private bool showOverlay = true;
        private long _fakeEntityId = 900000;

        private void OnGUI()
        {
            if (!showOverlay || runtimeRoot == null) return;
            GUILayout.BeginArea(new Rect(20, 20, 340, 470), GUI.skin.box);
            GUILayout.Label("SCORE DEBUG");
            GUILayout.Label("Initialized: " + runtimeRoot.IsInitialized);

            if (runtimeRoot.Runtime != null)
            {
                ScoreSnapshot s = runtimeRoot.Runtime.Snapshot;
                GUILayout.Label("State: " + s.State);
                GUILayout.Label("Enabled: " + s.ScoringEnabled);
                GUILayout.Label("Level: " + s.CurrentLevel);
                GUILayout.Label("Total: " + s.TotalScore);
                GUILayout.Label("Level Score: " + s.LevelScore);
                GUILayout.Label("Checkpoint: " + s.LevelStartTotalScore);

                if (GUILayout.Button("Start New Run")) runtimeRoot.StartRun();
                if (GUILayout.Button("Begin GL1")) runtimeRoot.BeginLevel(1);
                if (GUILayout.Button("Begin GL2")) runtimeRoot.BeginLevel(2);
                if (GUILayout.Button("Replay Current Level")) runtimeRoot.ReplayCurrentLevel();
                if (GUILayout.Button("+ Normal Zombie")) Award(ScoreActionId.NormalZombieDefeated);
                if (GUILayout.Button("+ Boss A")) Award(ScoreActionId.BossADefeated);
                if (GUILayout.Button("+ Boss B")) Award(ScoreActionId.BossBDefeated);
                if (GUILayout.Button("+ Boss C")) Award(ScoreActionId.BossCDefeated);
                if (GUILayout.Button("Scoring OFF")) runtimeRoot.SetScoringEnabled(false);
                if (GUILayout.Button("Scoring ON")) runtimeRoot.SetScoringEnabled(true);
            }
            GUILayout.EndArea();
        }

        private void Award(ScoreActionId action)
        {
            _fakeEntityId++;
            runtimeRoot.Award(action, _fakeEntityId);
        }
    }
}
