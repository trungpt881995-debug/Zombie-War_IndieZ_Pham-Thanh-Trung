using System;
using UnityEngine;
using ZombieWar.Features.Projectile.Factories;
using ZombieWar.Features.Projectile.Motion;
using ZombieWar.Features.Projectile.Registry;
using ZombieWar.Features.Projectile.Services;
using ZombieWar.Features.Projectile.Unity.Pooling;

namespace ZombieWar.Features.Projectile.Unity.Runtime
{
    [DisallowMultipleComponent]
    public sealed class ProjectileRuntimeRoot : MonoBehaviour
    {
        [SerializeField] private ProjectilePool pool;
        [SerializeField] private ProjectileSimulationDriver driver;

        private ActiveProjectileRegistry _registry;
        public IProjectileLauncher Launcher { get; private set; }
        public bool IsInitialized => Launcher != null;

        public void Initialize(IProjectileControllerFactory controllerFactory, IProjectileLauncherFactory launcherFactory)
        {
            if (IsInitialized) return;
            if (controllerFactory == null) throw new ArgumentNullException(nameof(controllerFactory));
            if (launcherFactory == null) throw new ArgumentNullException(nameof(launcherFactory));
            if (pool == null) throw new InvalidOperationException("ProjectilePool is not assigned.");
            if (driver == null) throw new InvalidOperationException("ProjectileSimulationDriver is not assigned.");

            _registry = new ActiveProjectileRegistry();
            pool.Initialize(controllerFactory, _registry);

            var gravity = new ZombieWar.Features.Projectile.Domain.ProjectileVector(Physics.gravity.x, Physics.gravity.y, Physics.gravity.z);
            var linear = new LinearLaunchVelocitySolver();
            var ballistic = new BallisticLaunchVelocitySolver(in gravity);
            var resolver = new ProjectileLaunchVelocityResolver(linear, ballistic);

            Launcher = launcherFactory.Create(pool, resolver);
            driver.Initialize(_registry);
        }

        public void CancelAll() => driver?.CancelAll();
    }
}
