using UnityEngine;
using ZombieWar.Features.GameState.Unity.Runtime;

namespace ZombieWar.Features.GameState.Unity.View
{
    [DisallowMultipleComponent]
    public sealed class GameStateDebugView : MonoBehaviour
    {
        [SerializeField] private GameStateRuntimeRoot runtimeRoot;
        [SerializeField] private bool showOverlay = true;

        private void OnGUI()
        {
            if (!showOverlay || runtimeRoot == null) return;
            GUILayout.BeginArea(new Rect(20, 20, 330, 430), GUI.skin.box);
            GUILayout.Label("GAME STATE DEBUG");
            GUILayout.Label($"Bound: {runtimeRoot.IsBound}");
            if (runtimeRoot.Runtime != null)
            {
                var s = runtimeRoot.Runtime.Snapshot;
                GUILayout.Label($"Current: {s.Current}");
                GUILayout.Label($"Previous: {s.Previous}");
                GUILayout.Label($"Sequence: {s.TransitionSequence}");
                GUILayout.Space(8);
                if (GUILayout.Button("Begin Gameplay")) runtimeRoot.BeginGameplay();
                if (GUILayout.Button("Pause")) runtimeRoot.Pause();
                if (GUILayout.Button("Resume")) runtimeRoot.Resume();
                if (GUILayout.Button("Game Over")) runtimeRoot.EnterGameOver();
                if (GUILayout.Button("Level Complete")) runtimeRoot.EnterLevelComplete();
                if (GUILayout.Button("End Game")) runtimeRoot.EnterEndGame();
                if (GUILayout.Button("Deactivate")) runtimeRoot.Deactivate();
            }
            GUILayout.EndArea();
        }
    }
}
