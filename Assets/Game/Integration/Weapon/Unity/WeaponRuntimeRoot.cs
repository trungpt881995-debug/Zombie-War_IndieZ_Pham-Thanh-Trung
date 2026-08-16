using System;
using GeneralCore.Architecture;
using GameplayCore.Entities;
using GameplayEntityId = GameplayCore.Entities.EntityId;
using GameplayCore.Time;
using UnityEngine;
using ZombieWar.Features.Projectile.Unity.Runtime;
using ZombieWar.Features.Soldier.Events;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Events;
using ZombieWar.Features.Weapon.Services;
using ZombieWar.Features.Weapon.Unity.Config;

namespace ZombieWar.Integration.Weapon.Unity
{
    [DisallowMultipleComponent]
    public sealed class WeaponRuntimeRoot : MonoBehaviour
    {
        private const int SoldierSlotCount = 4;

        [Header("Weapon Catalog")]
        [SerializeField]
        private WeaponCatalogConfig catalogConfig;

        [Header("Projectile Runtime")]
        [SerializeField]
        private ProjectileRuntimeRoot projectileRuntime;

        [SerializeField]
        private int pistolPoolKey = 1;

        [SerializeField]
        private int akPoolKey = 2;

        [SerializeField]
        private int shotgunPelletPoolKey = 3;

        [SerializeField]
        private int sniperPoolKey = 4;

        [SerializeField]
        private int grenadePoolKey = 5;

        [Header("Soldier Weapon Views by Slot 0..3")]
        [SerializeField]
        private SoldierWeaponView[] soldierWeaponViews =
            new SoldierWeaponView[SoldierSlotCount];

        // Migration compatibility for scenes authored before Weapon Asset Integration.
        // Keep the original serialized field name so existing scene references are not lost
        // on script reload. A configured SoldierWeaponView always takes precedence.
        [SerializeField, HideInInspector]
        private TransformWeaponMuzzleSource[] soldierMuzzles =
            new TransformWeaponMuzzleSource[SoldierSlotCount];

        [Header("Runtime")]
        [SerializeField]
        private bool tickSelectionCooldowns = true;

        private IWeaponRuntime _runtime;
        private IWeaponAttackService _attackService;
        private IGameplayClock _gameplayClock;
        private IWeaponProjectileBinding _projectileBinding;
        private IWeaponMuzzleRegistry _muzzleRegistry;
        private IDisposable _soldierAddedSubscription;
        private IDisposable _weaponSelectedSubscription;

        private readonly GameplayEntityId[] _registeredIds =
            new GameplayEntityId[SoldierSlotCount];

        private readonly bool[] _registered =
            new bool[SoldierSlotCount];

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
            if (IsInitialized)
            {
                return;
            }

            if (runtime == null)
            {
                throw new ArgumentNullException(nameof(runtime));
            }

            if (attackService == null)
            {
                throw new ArgumentNullException(nameof(attackService));
            }

            if (gameplayClock == null)
            {
                throw new ArgumentNullException(nameof(gameplayClock));
            }

            if (projectileBinding == null)
            {
                throw new ArgumentNullException(nameof(projectileBinding));
            }

            if (muzzleRegistry == null)
            {
                throw new ArgumentNullException(nameof(muzzleRegistry));
            }

            if (eventSubscriber == null)
            {
                throw new ArgumentNullException(nameof(eventSubscriber));
            }

            ValidateSceneReferences();

            _runtime = runtime;
            _attackService = attackService;
            _gameplayClock = gameplayClock;
            _projectileBinding = projectileBinding;
            _muzzleRegistry = muzzleRegistry;

            _runtime.Initialize(
                catalogConfig.CreateCatalog(),
                catalogConfig.InitialWeapon);

            var mapping = new WeaponProjectilePoolMapping(
                pistolPoolKey,
                akPoolKey,
                shotgunPelletPoolKey,
                sniperPoolKey,
                grenadePoolKey);

            _projectileBinding.Bind(
                projectileRuntime.Launcher,
                in mapping);

            _soldierAddedSubscription =
                eventSubscriber.Subscribe<SoldierAddedEvent>(OnSoldierAdded);

            _weaponSelectedSubscription =
                eventSubscriber.Subscribe<WeaponSelectedEvent>(OnWeaponSelected);

            // Initialization does not publish WeaponSelectedEvent, so synchronize the
            // scene presentation explicitly before any Soldier can fire.
            ApplyWeaponToAllViews(_runtime.CurrentWeapon);

            IsInitialized = true;
        }

        public WeaponSelectionResult SelectWeapon(WeaponType type)
        {
            return _runtime != null
                ? _runtime.TrySelect(type)
                : WeaponSelectionResult.Rejected(
                    WeaponType.Pistol,
                    WeaponSelectionRejectReason.NotInitialized);
        }

        public void SetGameplayEnabled(bool enabled)
        {
            if (!enabled)
            {
                _attackService?.ClearAll();
            }

            _runtime?.SetGameplayEnabled(enabled);
        }

        public void ResetForGameLevel()
        {
            _attackService?.ClearAll();
            _runtime?.ResetForGameLevel();

            // ResetForGameLevel restores the initial Weapon in the model without
            // publishing WeaponSelectedEvent. Mirror the authoritative state here.
            if (_runtime != null && _runtime.IsInitialized)
            {
                ApplyWeaponToAllViews(_runtime.CurrentWeapon);
            }
        }

        private void Update()
        {
            if (IsInitialized && tickSelectionCooldowns)
            {
                _runtime.Tick(
                    _gameplayClock != null
                        ? _gameplayClock.DeltaTime
                        : 0f);
            }
        }

        private void OnSoldierAdded(SoldierAddedEvent evt)
        {
            int slot = evt.SlotIndex;

            if (slot < 0 || slot >= SoldierSlotCount)
            {
                return;
            }

            if (_registered[slot])
            {
                _muzzleRegistry.Unregister(_registeredIds[slot]);
            }

            IWeaponMuzzleSource source = GetMuzzleSource(slot);
            _muzzleRegistry.Register(evt.SoldierId, source);

            _registeredIds[slot] = evt.SoldierId;
            _registered[slot] = true;
        }

        private void OnWeaponSelected(WeaponSelectedEvent evt)
        {
            ApplyWeaponToAllViews(evt.Current);
        }

        private void ApplyWeaponToAllViews(WeaponType weaponType)
        {
            if (soldierWeaponViews == null)
            {
                return;
            }

            for (int i = 0; i < soldierWeaponViews.Length; i++)
            {
                SoldierWeaponView view = soldierWeaponViews[i];

                if (view != null)
                {
                    view.ShowWeapon(weaponType);
                }
            }
        }

        private IWeaponMuzzleSource GetMuzzleSource(int slot)
        {
            SoldierWeaponView view = soldierWeaponViews[slot];

            if (view != null)
            {
                return view;
            }

            // Temporary migration fallback. Once all four SoldierWeaponView references
            // are authored in the scene, legacy TransformWeaponMuzzleSource components
            // are no longer used by WeaponRuntimeRoot.
            TransformWeaponMuzzleSource legacy =
                soldierMuzzles != null && slot < soldierMuzzles.Length
                    ? soldierMuzzles[slot]
                    : null;

            if (legacy != null)
            {
                return legacy;
            }

            throw new InvalidOperationException(
                $"No weapon muzzle source is configured for Soldier slot {slot}.");
        }

        private void ValidateSceneReferences()
        {
            if (catalogConfig == null)
            {
                throw new InvalidOperationException(
                    "WeaponCatalogConfig is not assigned.");
            }

            if (projectileRuntime == null || !projectileRuntime.IsInitialized)
            {
                throw new InvalidOperationException(
                    "ProjectileRuntimeRoot must be initialized before WeaponRuntimeRoot.");
            }

            if (soldierWeaponViews == null ||
                soldierWeaponViews.Length != SoldierSlotCount)
            {
                throw new InvalidOperationException(
                    $"Exactly {SoldierSlotCount} SoldierWeaponView slots are required.");
            }

            if (soldierMuzzles != null &&
                soldierMuzzles.Length != SoldierSlotCount)
            {
                throw new InvalidOperationException(
                    $"Legacy Soldier muzzle array must contain exactly {SoldierSlotCount} slots.");
            }

            for (int i = 0; i < SoldierSlotCount; i++)
            {
                SoldierWeaponView view = soldierWeaponViews[i];

                if (view != null)
                {
                    view.ValidateConfiguration();
                    continue;
                }

                TransformWeaponMuzzleSource legacy =
                    soldierMuzzles != null
                        ? soldierMuzzles[i]
                        : null;

                if (legacy == null)
                {
                    throw new InvalidOperationException(
                        $"Soldier weapon source at slot {i} is not assigned. " +
                        $"Assign {nameof(SoldierWeaponView)} for Weapon Asset Integration.");
                }
            }
        }

        private void OnDestroy()
        {
            _soldierAddedSubscription?.Dispose();
            _weaponSelectedSubscription?.Dispose();

            _soldierAddedSubscription = null;
            _weaponSelectedSubscription = null;

            if (_muzzleRegistry != null)
            {
                for (int i = 0; i < _registered.Length; i++)
                {
                    if (_registered[i])
                    {
                        _muzzleRegistry.Unregister(_registeredIds[i]);
                    }
                }
            }

            _attackService?.ClearAll();
            _projectileBinding?.Unbind();
            IsInitialized = false;
        }
    }
}
