using GameplayCore.Time;
using UnityEngine;
using ZombieWar.Features.Zombie.Registry;

namespace ZombieWar.Integration.Zombie.Unity
{
    [DisallowMultipleComponent]
    public sealed class ZombieSimulationDriver : MonoBehaviour
    {
        private IActiveZombieRegistry _registry;
        private IGameplayClock _clock;
        public void Initialize(IActiveZombieRegistry registry, IGameplayClock clock)
        {
            _registry = registry; _clock = clock;
        }
        private void Update()
        {
            if (_registry == null || _clock == null) return;
            float dt = _clock.DeltaTime;
            var active = _registry.Active;
            for (int i = active.Count - 1; i >= 0; i--)
                active[i].Tick(dt);
        }
    }
}
