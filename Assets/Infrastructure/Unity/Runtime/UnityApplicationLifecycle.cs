using GeneralCore.Runtime;
using UnityEngine;

namespace ZombieWar.Infrastructure.Unity
{
    public sealed class UnityApplicationLifecycle : MonoBehaviour, IApplicationLifecycle
    {
        public bool IsFocused { get; private set; } = true;
        public bool IsPaused { get; private set; }
        private void OnApplicationFocus(bool focus) => IsFocused = focus;
        private void OnApplicationPause(bool pause) => IsPaused = pause;
    }
}
