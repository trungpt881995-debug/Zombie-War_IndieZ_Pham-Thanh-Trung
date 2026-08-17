using System;
using GameplayEntityId = GameplayCore.Entities.EntityId;
using UnityEngine;
using UnityEngine.Pool;
using ZombieWar.Features.Health.Factories;
using ZombieWar.Features.Targeting.Registry;
using ZombieWar.Features.Boss.Catalog;
using ZombieWar.Features.Boss.Domain;
using ZombieWar.Features.Boss.Factories;
using ZombieWar.Features.Boss.Ports;
using ZombieWar.Features.Boss.Registry;
using ZombieWar.Features.Boss.Unity.Config;

namespace ZombieWar.Integration.Boss.Unity
{
    [DisallowMultipleComponent]
    public sealed class BossPoolRegistry :
        MonoBehaviour,
        IBossSpawnExecutor
    {
        [SerializeField] private BossRuntimeHost bossAPrefab;
        [SerializeField] private BossRuntimeHost bossBPrefab;
        [SerializeField] private BossRuntimeHost bossCPrefab;
        [SerializeField] private Transform poolRoot;
        [SerializeField] private bool allowRuntimeExpansion = false;

        private ObjectPool<BossRuntimeHost> _a;
        private ObjectPool<BossRuntimeHost> _b;
        private ObjectPool<BossRuntimeHost> _c;

        private IBossFactory _factory;
        private IHealthFactory _health;
        private ITargetRegistry _targets;
        private IActiveBossRegistry _active;
        private IBossCatalog _catalog;
        private bool _initialized;

        public void Initialize(
            IBossFactory factory,
            IHealthFactory health,
            ITargetRegistry targets,
            IActiveBossRegistry active,
            IBossCatalog catalog)
        {
            if (_initialized)
            {
                return;
            }

            _factory = factory
                ?? throw new ArgumentNullException(nameof(factory));

            _health = health
                ?? throw new ArgumentNullException(nameof(health));

            _targets = targets
                ?? throw new ArgumentNullException(nameof(targets));

            _active = active
                ?? throw new ArgumentNullException(nameof(active));

            _catalog = catalog
                ?? throw new ArgumentNullException(nameof(catalog));

            if (bossAPrefab == null ||
                bossBPrefab == null ||
                bossCPrefab == null)
            {
                throw new InvalidOperationException(
                    "Boss A/B/C prefabs must all be assigned.");
            }

            if (poolRoot == null)
            {
                poolRoot = transform;
            }

            _a = CreatePool(BossId.BossA, bossAPrefab);
            _b = CreatePool(BossId.BossB, bossBPrefab);
            _c = CreatePool(BossId.BossC, bossCPrefab);

            Prewarm(_a);
            Prewarm(_b);
            Prewarm(_c);

            _initialized = true;
        }

        public bool TrySpawnPlan(in BossSpawnPlan plan)
        {
            if (!_initialized ||
                plan.Count < 1 ||
                plan.Count > 2)
            {
                return false;
            }

            BossSpawnRequest r0 = plan.First;
            ObjectPool<BossRuntimeHost> p0 = Pool(r0.BossId);

            if (p0 == null ||
                (!allowRuntimeExpansion && p0.CountInactive <= 0))
            {
                return false;
            }

            ObjectPool<BossRuntimeHost> p1 = null;
            BossSpawnRequest r1 = default;

            if (plan.Count == 2)
            {
                r1 = plan.Second;
                p1 = Pool(r1.BossId);

                if (p1 == null ||
                    (!allowRuntimeExpansion && p1.CountInactive <= 0))
                {
                    return false;
                }
            }

            BossRuntimeHost h0 = p0.Get();
            BossRuntimeHost h1 = null;
            bool active0 = false;
            bool active1 = false;

            try
            {
                if (plan.Count == 2)
                {
                    h1 = p1.Get();
                }

                if (!Activate(h0, in r0))
                {
                    p0.Release(h0);

                    if (h1 != null)
                    {
                        p1.Release(h1);
                    }

                    return false;
                }

                active0 = true;

                if (plan.Count == 2 &&
                    !Activate(h1, in r1))
                {
                    if (active0)
                    {
                        h0.Controller.Cancel();
                    }
                    else
                    {
                        p0.Release(h0);
                    }

                    if (h1 != null &&
                        h1.Controller != null &&
                        h1.Controller.IsActive)
                    {
                        h1.Controller.Cancel();
                    }
                    else if (h1 != null)
                    {
                        p1.Release(h1);
                    }

                    return false;
                }

                active1 = plan.Count == 2;
                return true;
            }
            catch
            {
                if (active1 && h1 != null)
                {
                    h1.Controller.Cancel();
                }
                else if (h1 != null)
                {
                    p1?.Release(h1);
                }

                if (active0)
                {
                    h0.Controller.Cancel();
                }
                else if (h0 != null)
                {
                    p0.Release(h0);
                }

                throw;
            }
        }

        internal void Release(
            BossRuntimeHost host,
            GameplayEntityId id)
        {
            if (host == null || !_initialized)
            {
                return;
            }

            _active.Remove(id);

            Pool(
                host.Controller != null
                    ? host.Controller.BossId
                    : BossId.None)
                ?.Release(host);
        }

        private bool Activate(
            BossRuntimeHost host,
            in BossSpawnRequest request)
        {
            if (host == null ||
                !_catalog.TryGet(
                    request.BossId,
                    out BossDefinition definition))
            {
                return false;
            }

            GameplayEntityId id =
                host.Controller.Activate(
                    in definition,
                    in request);

            if (!_active.Add(host.Controller))
            {
                host.Controller.DeactivateForPool();
                return false;
            }

            return true;
        }

        private ObjectPool<BossRuntimeHost> CreatePool(
            BossId id,
            BossRuntimeHost prefab)
        {
            return new ObjectPool<BossRuntimeHost>(
                () => CreateHost(id, prefab),
                host =>
                {
                    if (host != null)
                    {
                        host.gameObject.SetActive(true);
                    }
                },
                host =>
                {
                    if (host == null)
                    {
                        return;
                    }

                    host.Controller?.DeactivateForPool();
                    host.gameObject.SetActive(false);
                },
                host =>
                {
                    if (host != null)
                    {
                        Destroy(host.gameObject);
                    }
                },
                true,
                1,
                1);
        }

        private BossRuntimeHost CreateHost(
            BossId id,
            BossRuntimeHost prefab)
        {
            BossRuntimeHost host =
                Instantiate(prefab, poolRoot);

            host.gameObject.SetActive(false);

            var health =
                new BossHealthAdapter(_health);

            var registration =
                new BossTargetRegistrationAdapter(_targets);

            var poolReturn =
                new BossPoolReturnAdapter(this, host);

            var controller =
                _factory.Create(
                    host.View,
                    host.Motor,
                    health,
                    registration,
                    poolReturn);

            // Important:
            // Targeting receives BossRuntimeHost.GetTargetPosition(), while
            // BossController.Position remains the root position for Boss logic.
            var bridge =
                new BossCombatBridge(
                    controller,
                    host.GetTargetPosition);

            registration.Bind(bridge);
            host.Bind(controller, bridge);

            return host;
        }

        private static void Prewarm(
            ObjectPool<BossRuntimeHost> pool)
        {
            BossRuntimeHost host = pool.Get();
            pool.Release(host);
        }

        private ObjectPool<BossRuntimeHost> Pool(BossId id)
        {
            switch (id)
            {
                case BossId.BossA:
                    return _a;

                case BossId.BossB:
                    return _b;

                case BossId.BossC:
                    return _c;

                default:
                    return null;
            }
        }
    }
}
