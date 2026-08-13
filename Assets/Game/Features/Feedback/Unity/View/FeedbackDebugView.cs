using UnityEngine;
using ZombieWar.Features.Feedback.Domain;
using ZombieWar.Features.Feedback.Services;
using ZombieWar.Features.Feedback.Unity.Runtime;

namespace ZombieWar.Features.Feedback.Unity.View
{
    public sealed class FeedbackDebugView : MonoBehaviour
    {
        [SerializeField] private FeedbackRuntimeRoot runtimeRoot;
        [SerializeField] private bool showOverlay = true;

        private void OnGUI()
        {
            if (!showOverlay ||
                runtimeRoot == null ||
                !runtimeRoot.IsBound)
            {
                return;
            }

            IFeedbackRuntime runtime = runtimeRoot.Runtime;
            FeedbackSnapshot snapshot = runtime.Snapshot;

            GUILayout.BeginArea(
                new Rect(12f, 12f, 260f, 410f),
                GUI.skin.box);

            GUILayout.Label("FEEDBACK DEBUG");
            GUILayout.Label($"Mode: {snapshot.Mode}");
            GUILayout.Label($"Accepted: {snapshot.AcceptedCount}");
            GUILayout.Label($"Rejected: {snapshot.RejectedCount}");
            GUILayout.Label($"Sequence: {snapshot.Sequence}");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Playing"))
            {
                runtime.SetMode(FeedbackRuntimeMode.Playing);
            }

            if (GUILayout.Button("Suspended"))
            {
                runtime.SetMode(FeedbackRuntimeMode.Suspended);
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Terminal"))
            {
                runtime.SetMode(FeedbackRuntimeMode.TerminalDrain);
            }

            if (GUILayout.Button("Inactive"))
            {
                runtime.SetMode(FeedbackRuntimeMode.Inactive);
            }

            GUILayout.EndHorizontal();

            DrawButton(runtime, "Pistol", FeedbackId.PistolShot);
            DrawButton(runtime, "AK", FeedbackId.AKShot);
            DrawButton(runtime, "Shotgun", FeedbackId.ShotgunShot);
            DrawButton(runtime, "Explosion", FeedbackId.GrenadeExplosion);
            DrawButton(runtime, "Soldier Damage", FeedbackId.SoldierDamaged);
            DrawButton(runtime, "Boss Defeated", FeedbackId.BossDefeated);
            DrawButton(runtime, "Game Over", FeedbackId.GameOver);
            DrawButton(runtime, "Level Complete", FeedbackId.LevelComplete);
            DrawButton(runtime, "End Game", FeedbackId.EndGame);

            if (GUILayout.Button("Cancel All"))
            {
                runtime.CancelAll();
            }

            GUILayout.EndArea();
        }

        private static void DrawButton(
            IFeedbackRuntime runtime,
            string label,
            FeedbackId id)
        {
            if (!GUILayout.Button(label))
            {
                return;
            }

            var request = new FeedbackRequest(id);
            runtime.Play(in request);
        }
    }
}
