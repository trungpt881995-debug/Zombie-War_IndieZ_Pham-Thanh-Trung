using UnityEngine;
using ZombieWar.Features.Projectile.Unity.Runtime;
using ZombieWar.Integration.GameState.Runtime;

namespace ZombieWar.Integration.GameState.Unity
{
    [DisallowMultipleComponent]
    public sealed class ProjectileGameStateGateTarget : MonoBehaviour, IGameStateRuntimeGateTarget
    {
        [SerializeField] private ProjectileSimulationDriver simulationDriver;
        public void SetGameplayEnabled(bool enabled)
        {
            if (simulationDriver != null) simulationDriver.enabled = enabled;
        }
    }
}
