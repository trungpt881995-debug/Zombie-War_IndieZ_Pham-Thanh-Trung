using System;
using GameplayCore.Entities;
using GameplayEntityId = GameplayCore.Entities.EntityId;
using UnityEngine;
using UnityEngine.Pool;
using ZombieWar.Features.Health.Factories;
using ZombieWar.Features.Targeting.Registry;
using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Factories;
using ZombieWar.Features.Zombie.Registry;
using ZombieWar.Features.Zombie.Unity.Config;

namespace ZombieWar.Integration.Zombie.Unity
{
    [DisallowMultipleComponent]
    public sealed class ZombiePool : MonoBehaviour
    {
        [SerializeField] private ZombieRuntimeHost prefab;
        [SerializeField] private Transform poolRoot;
        [SerializeField, Min(0)] private int prewarmCount = 32;
        [SerializeField, Min(1)] private int maxSize = 128;
        [SerializeField] private bool allowRuntimeExpansion = false;

        private ObjectPool<ZombieRuntimeHost> _pool;
        private IZombieFactory _factory;
        private IHealthFactory _healthFactory;
        private ITargetRegistry _targetRegistry;
        private IActiveZombieRegistry _active;
        private ZombieDefinition _definition;
        private bool _initialized;

        public int CountInactive => _pool != null ? _pool.CountInactive : 0;
        public int CountAll => _pool != null ? _pool.CountAll : 0;

        public void Initialize(
            IZombieFactory factory,
            IHealthFactory healthFactory,
            ITargetRegistry targetRegistry,
            IActiveZombieRegistry active,
            ZombieConfig config)
        {
            if (_initialized) return;
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _healthFactory = healthFactory ?? throw new ArgumentNullException(nameof(healthFactory));
            _targetRegistry = targetRegistry ?? throw new ArgumentNullException(nameof(targetRegistry));
            _active = active ?? throw new ArgumentNullException(nameof(active));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (prefab == null) throw new InvalidOperationException("Zombie prefab is not assigned.");
            if (poolRoot == null) poolRoot = transform;
            _definition = config.CreateDefinition();
            int capacity = Math.Max(1, prewarmCount);
            int maximum = Math.Max(capacity, maxSize);
            _pool = new ObjectPool<ZombieRuntimeHost>(Create, OnGet, OnRelease, OnDestroyHost, true, capacity, maximum);
            if (prewarmCount > 0)
            {
                var temp = new ZombieRuntimeHost[Math.Min(prewarmCount, maximum)];
                for (int i = 0; i < temp.Length; i++) temp[i] = _pool.Get();
                for (int i = 0; i < temp.Length; i++) _pool.Release(temp[i]);
            }
            _initialized = true;
        }

        public bool TrySpawn(in ZombieSpawnRequest request, out GameplayEntityId entityId)
        {
            Debug.Log("hit TrySpawn!!!");
            entityId = default;
            if (!_initialized) return false;
            if (!allowRuntimeExpansion && _pool.CountInactive <= 0) return false;
            ZombieRuntimeHost host = _pool.Get();
            try
            {
                entityId = host.Controller.Activate(in _definition, in request);
                if (!_active.Add(host.Controller))
                {
                    host.Controller.Cancel();
                    entityId = default;
                    return false;
                }
                return true;
            }
            catch
            {
                _pool.Release(host);
                throw;
            }
        }

        internal void Release(ZombieRuntimeHost host, GameplayEntityId entityId)
        {
            if (host == null || !_initialized) return;
            _active.Remove(entityId);
            _pool.Release(host);
        }

        public void CancelAll()
        {
            for (int i = _active.Active.Count - 1; i >= 0; i--)
                _active.Active[i].Cancel();
        }

        private ZombieRuntimeHost Create()
        {
            ZombieRuntimeHost host = Instantiate(prefab, poolRoot);
            host.gameObject.SetActive(false);
            var health = new ZombieHealthAdapter(_healthFactory);
            var registration = new ZombieTargetRegistrationAdapter(_targetRegistry);
            var poolReturn = new ZombiePoolReturnAdapter(this, host);
            var controller = _factory.Create(host.View, host.Motor, health, registration, poolReturn);
            var bridge = new ZombieCombatBridge(controller);
            registration.Bind(bridge);
            host.Bind(controller, bridge);
            return host;
        }
        private static void OnGet(ZombieRuntimeHost host) { if (host != null) host.gameObject.SetActive(true); }
        private static void OnRelease(ZombieRuntimeHost host)
        {
            if (host == null) return;
            host.Controller?.DeactivateForPool();
            host.gameObject.SetActive(false);
        }
        private static void OnDestroyHost(ZombieRuntimeHost host) { if (host != null) Destroy(host.gameObject); }
    }
}
