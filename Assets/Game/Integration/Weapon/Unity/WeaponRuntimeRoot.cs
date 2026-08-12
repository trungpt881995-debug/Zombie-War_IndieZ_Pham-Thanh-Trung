using System;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using GameplayEntityId = GameplayCore.Entities.EntityId;
using GameplayCore.Time;
using UnityEngine;
using ZombieWar.Features.Projectile.Unity.Runtime;
using ZombieWar.Features.Soldier.Events;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Services;
using ZombieWar.Features.Weapon.Unity.Config;

namespace ZombieWar.Integration.Weapon.Unity
{
    [DisallowMultipleComponent]
    public sealed class WeaponRuntimeRoot : MonoBehaviour
    {
        [Header("Weapon Catalog")]
        [SerializeField] private WeaponCatalogConfig catalogConfig;

        [Header("Projectile Runtime")]
        [SerializeField] private ProjectileRuntimeRoot projectileRuntime;
        [SerializeField] private int pistolPoolKey = 1;
        [SerializeField] private int akPoolKey = 2;
        [SerializeField] private int shotgunPelletPoolKey = 3;
        [SerializeField] private int sniperPoolKey = 4;
        [SerializeField] private int grenadePoolKey = 5;

        [Header("Soldier Muzzles by Slot 0..3")]
        [SerializeField] private TransformWeaponMuzzleSource[] soldierMuzzles =
            new TransformWeaponMuzzleSource[4];

        [Header("Runtime")]
        [SerializeField] private bool tickSelectionCooldowns = true;

        private IWeaponRuntime _runtime;
        private IWeaponAttackService _attackService;
        private IGameplayClock _gameplayClock;
        private IWeaponProjectileBinding _projectileBinding;
        private IWeaponMuzzleRegistry _muzzleRegistry;
        private IDisposable _soldierAddedSubscription;
        private readonly GameplayEntityId[] _registeredIds = new GameplayEntityId[4];
        private readonly bool[] _registered = new bool[4];

        public bool IsInitialized { get; private set; }
        public IWeaponRuntime Runtime => _runtime;

        public void Initialize(
            IWeaponRuntime runtime,
            IWeaponAttackService attackService,
            IGameplayClock gameplayClock,
            IWeaponProjectileBinding projectileBinding,
            IWeaponMuzzleRegistry muzzleRegistry,
            IEventSubscriber eventSubscriber)
        {
            if (IsInitialized) return;
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (attackService == null) throw new ArgumentNullException(nameof(attackService));
            if (gameplayClock == null) throw new ArgumentNullException(nameof(gameplayClock));
            if (projectileBinding == null) throw new ArgumentNullException(nameof(projectileBinding));
            if (muzzleRegistry == null) throw new ArgumentNullException(nameof(muzzleRegistry));
            if (eventSubscriber == null) throw new ArgumentNullException(nameof(eventSubscriber));
            if (catalogConfig == null) throw new InvalidOperationException("WeaponCatalogConfig is not assigned.");
            if (projectileRuntime == null || !projectileRuntime.IsInitialized)
                throw new InvalidOperationException("ProjectileRuntimeRoot must be initialized before WeaponRuntimeRoot.");
            if (soldierMuzzles == null || soldierMuzzles.Length != 4)
                throw new InvalidOperationException("Exactly four Soldier muzzle sources are required.");
            for (int i = 0; i < soldierMuzzles.Length; i++)
                if (soldierMuzzles[i] == null)
                    throw new InvalidOperationException($"Soldier muzzle source at slot {i} is not assigned.");

            _runtime = runtime;
            _attackService = attackService;
            _gameplayClock = gameplayClock;
            _projectileBinding = projectileBinding;
            _muzzleRegistry = muzzleRegistry;
            _runtime.Initialize(catalogConfig.CreateCatalog(), catalogConfig.InitialWeapon);

            var mapping = new WeaponProjectilePoolMapping(
                pistolPoolKey, akPoolKey, shotgunPelletPoolKey, sniperPoolKey, grenadePoolKey);
            _projectileBinding.Bind(projectileRuntime.Launcher, in mapping);

            _soldierAddedSubscription = eventSubscriber.Subscribe<SoldierAddedEvent>(OnSoldierAdded);
            IsInitialized = true;
        }

        public WeaponSelectionResult SelectWeapon(WeaponType type)
        {
            return _runtime != null
                ? _runtime.TrySelect(type)
                : WeaponSelectionResult.Rejected(WeaponType.Pistol, WeaponSelectionRejectReason.NotInitialized);
        }

        public void SetGameplayEnabled(bool enabled)
        {
            if (!enabled) _attackService?.ClearAll();
            _runtime?.SetGameplayEnabled(enabled);
        }

        public void ResetForGameLevel()
        {
            _attackService?.ClearAll();
            _runtime?.ResetForGameLevel();
        }

        private void Update()
        {
            if (IsInitialized && tickSelectionCooldowns)
                _runtime.Tick(_gameplayClock != null ? _gameplayClock.DeltaTime : 0f);
        }

        private void OnSoldierAdded(SoldierAddedEvent evt)
        {
            int slot = evt.SlotIndex;
            if (slot < 0 || slot >= soldierMuzzles.Length) return;
            if (_registered[slot]) _muzzleRegistry.Unregister(_registeredIds[slot]);
            _muzzleRegistry.Register(evt.SoldierId, soldierMuzzles[slot]);
            _registeredIds[slot] = evt.SoldierId;
            _registered[slot] = true;
        }

        private void OnDestroy()
        {
            _soldierAddedSubscription?.Dispose();
            if (_muzzleRegistry != null)
            {
                for (int i = 0; i < _registered.Length; i++)
                    if (_registered[i]) _muzzleRegistry.Unregister(_registeredIds[i]);
            }
            _attackService?.ClearAll();
            _projectileBinding?.Unbind();
            IsInitialized = false;
        }
    }
}
