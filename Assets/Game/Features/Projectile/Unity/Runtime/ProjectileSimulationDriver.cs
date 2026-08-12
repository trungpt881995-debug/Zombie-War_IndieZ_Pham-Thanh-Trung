using UnityEngine;
using ZombieWar.Features.Projectile.Controller;
using ZombieWar.Features.Projectile.Registry;

namespace ZombieWar.Features.Projectile.Unity.Runtime
{
    [DisallowMultipleComponent]
    public sealed class ProjectileSimulationDriver : MonoBehaviour
    {
        private IActiveProjectileRegistry _registry;
        public bool IsInitialized => _registry != null;

        public void Initialize(IActiveProjectileRegistry registry) => _registry = registry;

        private void FixedUpdate()
        {
            if (_registry == null) return;
            float dt = Time.fixedDeltaTime;
            for (int i = _registry.Count - 1; i >= 0; i--)
            {
                ProjectileController projectile = _registry.GetAt(i);
                if (projectile != null) projectile.Tick(dt);
            }
        }

        public void CancelAll()
        {
            if (_registry == null) return;
            while (_registry.Count > 0)
            {
                ProjectileController projectile = _registry.GetAt(_registry.Count - 1);
                if (projectile == null) break;
                projectile.Cancel();
            }
        }
    }
}
