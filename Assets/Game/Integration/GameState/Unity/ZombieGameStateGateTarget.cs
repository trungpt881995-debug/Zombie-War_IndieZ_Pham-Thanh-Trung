using UnityEngine;
using ZombieWar.Integration.GameState.Runtime;
using ZombieWar.Integration.Zombie.Unity;

namespace ZombieWar.Integration.GameState.Unity
{
    [DisallowMultipleComponent]
    public sealed class ZombieGameStateGateTarget : MonoBehaviour, IGameStateRuntimeGateTarget
    {
        [SerializeField] private ZombieRuntimeRoot runtimeRoot;
        public void SetGameplayEnabled(bool enabled)
        {
            if (runtimeRoot != null) runtimeRoot.SetGameplayEnabled(enabled);
        }
    }
}
