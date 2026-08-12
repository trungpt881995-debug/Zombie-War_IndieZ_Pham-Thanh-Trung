using System;
using GeneralCore.Architecture;
using GameplayCore.Damage;
using GameplayCore.Entities;
using GameplayEntityId = GameplayCore.Entities.EntityId;
using GameplayCore.Time;
using UnityEngine;
using ZombieWar.Features.Health.Factories;
using ZombieWar.Features.Soldier.Events;
using ZombieWar.Features.Targeting.Registry;
using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Factories;
using ZombieWar.Features.Zombie.Registry;
using ZombieWar.Features.Zombie.Unity.Config;

namespace ZombieWar.Integration.Zombie.Unity
{
    [DisallowMultipleComponent]
    public sealed class ZombieRuntimeRoot : MonoBehaviour
    {
        [SerializeField] private ZombieConfig zombieConfig;
        [SerializeField] private ZombiePool zombiePool;
        [SerializeField] private ZombieSimulationDriver simulationDriver;
        [SerializeField] private TransformZombieTargetSource[] soldierTargets = new TransformZombieTargetSource[4];
        [SerializeField] private MonoBehaviour sharedSoldierGroupDamageableBehaviour;

        private IActiveZombieRegistry _active;
        private IZombieSoldierTargetRegistry _soldierRegistry;
        private IZombieAttackBinding _attackBinding;
        private IDisposable _soldierAddedSubscription;
        private readonly GameplayEntityId[] _registeredIds = new GameplayEntityId[4];
        private readonly bool[] _registered = new bool[4];
        public bool IsInitialized { get; private set; }
        public int ActiveCount => _active != null ? _active.Count : 0;

        public void Initialize(
            IZombieFactory zombieFactory,
            IHealthFactory healthFactory,
            ITargetRegistry targetRegistry,
            IGameplayClock gameplayClock,
            IZombieSoldierTargetRegistry soldierRegistry,
            IZombieAttackBinding attackBinding,
            IEventSubscriber eventSubscriber)
        {
            if (IsInitialized) return;
            if (zombieFactory == null) throw new ArgumentNullException(nameof(zombieFactory));
            if (healthFactory == null) throw new ArgumentNullException(nameof(healthFactory));
            if (targetRegistry == null) throw new ArgumentNullException(nameof(targetRegistry));
            if (gameplayClock == null) throw new ArgumentNullException(nameof(gameplayClock));
            _soldierRegistry = soldierRegistry ?? throw new ArgumentNullException(nameof(soldierRegistry));
            _attackBinding = attackBinding ?? throw new ArgumentNullException(nameof(attackBinding));
            if (eventSubscriber == null) throw new ArgumentNullException(nameof(eventSubscriber));
            if (zombieConfig == null) throw new InvalidOperationException("ZombieConfig is not assigned.");
            if (zombiePool == null) throw new InvalidOperationException("ZombiePool is not assigned.");
            if (simulationDriver == null) throw new InvalidOperationException("ZombieSimulationDriver is not assigned.");
            if (soldierTargets == null || soldierTargets.Length != 4) throw new InvalidOperationException("Exactly four Soldier target sources are required.");
            for (int i = 0; i < soldierTargets.Length; i++)
                if (soldierTargets[i] == null) throw new InvalidOperationException($"Soldier target source at slot {i} is not assigned.");

            _active = new ActiveZombieRegistry();
            zombiePool.Initialize(zombieFactory, healthFactory, targetRegistry, _active, zombieConfig);
            simulationDriver.Initialize(_active, gameplayClock);
            _soldierAddedSubscription = eventSubscriber.Subscribe<SoldierAddedEvent>(OnSoldierAdded);

            if (sharedSoldierGroupDamageableBehaviour != null)
            {
                IDamageable damageable = sharedSoldierGroupDamageableBehaviour as IDamageable;
                if (damageable == null) throw new InvalidOperationException("Shared Soldier Group damageable Behaviour must implement IDamageable.");
                _attackBinding.BindSharedSoldierGroup(damageable);
            }
            IsInitialized = true;
        }

        public void BindSharedSoldierGroupDamageable(IDamageable damageable) => _attackBinding?.BindSharedSoldierGroup(damageable);

        public bool TrySpawn(Vector3 worldPosition, out GameplayEntityId zombieId)
        {
            zombieId = default;
            if (!IsInitialized) return false;
            var p = new ZombiePoint(worldPosition.x, worldPosition.y, worldPosition.z);
            var request = new ZombieSpawnRequest(in p);
            return zombiePool.TrySpawn(in request, out zombieId);
        }

        public void SetGameplayEnabled(bool enabled)
        {
            if (_active == null) return;
            var active = _active.Active;
            for (int i = active.Count - 1; i >= 0; i--) active[i].SetGameplayEnabled(enabled);
        }
        public void CancelAll() => zombiePool?.CancelAll();

        private void OnSoldierAdded(SoldierAddedEvent evt)
        {
            int slot = evt.SlotIndex;
            if (slot < 0 || slot >= soldierTargets.Length) return;
            if (_registered[slot]) _soldierRegistry.Unregister(_registeredIds[slot]);
            _soldierRegistry.Register(evt.SoldierId, soldierTargets[slot]);
            _registeredIds[slot] = evt.SoldierId;
            _registered[slot] = true;
        }

        private void OnDestroy()
        {
            _soldierAddedSubscription?.Dispose();
            zombiePool?.CancelAll();
            if (_soldierRegistry != null)
            {
                for (int i = 0; i < _registered.Length; i++) if (_registered[i]) _soldierRegistry.Unregister(_registeredIds[i]);
            }
            _attackBinding?.Unbind();
            IsInitialized = false;
        }
    }
}
