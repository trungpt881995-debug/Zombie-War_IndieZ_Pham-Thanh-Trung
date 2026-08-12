using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using ZombieWar.Features.Projectile.Controller;
using ZombieWar.Features.Projectile.Domain;
using ZombieWar.Features.Projectile.Factories;
using ZombieWar.Features.Projectile.Ports;
using ZombieWar.Features.Projectile.Registry;
using ZombieWar.Features.Projectile.Unity.Collision;
using ZombieWar.Features.Projectile.Unity.View;

namespace ZombieWar.Features.Projectile.Unity.Pooling
{
    [DisallowMultipleComponent]
    public sealed class ProjectilePool : MonoBehaviour, IProjectilePool
    {
        [Serializable]
        public sealed class Entry
        {
            public int poolKey;
            public ProjectileView prefab;
            [Min(1)] public int prewarmCount = 16;
            [Min(1)] public int maxSize = 64;
            public bool allowRuntimeExpansion;
        }

        private sealed class RuntimeEntry
        {
            public ProjectilePoolKey Key;
            public Entry Config;
            public ObjectPool<ProjectileController> Pool;
        }

        [SerializeField] private Transform poolRoot;
        [SerializeField] private Entry[] entries = Array.Empty<Entry>();

        private readonly Dictionary<ProjectilePoolKey, RuntimeEntry> _runtime =
            new Dictionary<ProjectilePoolKey, RuntimeEntry>();
        private IProjectileControllerFactory _controllerFactory;
        private IActiveProjectileRegistry _registry;
        private bool _initialized;

        public void Initialize(
            IProjectileControllerFactory controllerFactory,
            IActiveProjectileRegistry registry)
        {
            if (_initialized) return;
            _controllerFactory = controllerFactory ?? throw new ArgumentNullException(nameof(controllerFactory));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            if (poolRoot == null) poolRoot = transform;

            for (int i = 0; i < entries.Length; i++)
                Build(entries[i]);
            _initialized = true;
        }

        public ProjectileController Acquire(ProjectilePoolKey key)
        {
            if (!_runtime.TryGetValue(key, out RuntimeEntry entry)) return null;
            if (!entry.Config.allowRuntimeExpansion && entry.Pool.CountInactive <= 0)
                return null;
            return entry.Pool.Get();
        }

        public void Release(ProjectilePoolKey key, ProjectileController projectile)
        {
            if (projectile == null) return;
            if (!_runtime.TryGetValue(key, out RuntimeEntry entry))
            {
                projectile.View.Deactivate();
                projectile.ResetForPool();
                return;
            }
            entry.Pool.Release(projectile);
        }

        private void Build(Entry config)
        {
            if (config == null || config.prefab == null) return;
            if (config.poolKey < 0) throw new InvalidOperationException("Projectile pool key must be >= 0.");
            var key = new ProjectilePoolKey(config.poolKey);
            if (_runtime.ContainsKey(key))
                throw new InvalidOperationException($"Duplicate ProjectilePoolKey: {config.poolKey}");

            int max = Mathf.Max(config.maxSize, config.prewarmCount, 1);
            RuntimeEntry runtime = null;
            runtime = new RuntimeEntry
            {
                Key = key,
                Config = config
            };

            runtime.Pool = new ObjectPool<ProjectileController>(
                createFunc: () => CreateController(config),
                actionOnGet: null,
                actionOnRelease: controller =>
                {
                    controller.View.Deactivate();
                    controller.ResetForPool();
                },
                actionOnDestroy: controller =>
                {
                    if (controller != null && controller.View is ProjectileView view && view != null)
                        Destroy(view.gameObject);
                },
                collectionCheck: true,
                defaultCapacity: Mathf.Max(1, config.prewarmCount),
                maxSize: max);

            _runtime.Add(key, runtime);
            Prewarm(runtime, Mathf.Min(config.prewarmCount, max));
        }

        private ProjectileController CreateController(Entry config)
        {
            ProjectileView view = Instantiate(config.prefab, poolRoot);
            ProjectileCollisionRelay relay = view.GetComponent<ProjectileCollisionRelay>();
            if (relay == null) relay = view.gameObject.AddComponent<ProjectileCollisionRelay>();
            ProjectileController controller = _controllerFactory.Create(view, this, _registry);
            relay.Bind(controller);
            view.Deactivate();
            return controller;
        }

        private static void Prewarm(RuntimeEntry entry, int count)
        {
            if (count <= 0) return;
            var temp = new ProjectileController[count];
            for (int i = 0; i < count; i++) temp[i] = entry.Pool.Get();
            for (int i = 0; i < count; i++) entry.Pool.Release(temp[i]);
        }
    }
}
