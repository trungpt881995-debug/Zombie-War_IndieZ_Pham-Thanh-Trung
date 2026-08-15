using System;
using GeneralCore.Architecture;
using EntityId = GameplayCore.Entities.EntityId;
using UnityEngine;
using ZombieWar.Features.Soldier.Events;
using ZombieWar.Features.Soldier.View;

namespace ZombieWar.Integration.Soldier.Animation.Unity
{
    [DisallowMultipleComponent]
    public sealed class SoldierAnimationRuntimeRoot :
        MonoBehaviour
    {
        [Header("Soldier Views by Slot 0..3")]
        [SerializeField]
        private SoldierView[] soldierViews =
            new SoldierView[4];

        private ISoldierWeaponAnimationRegistry _registry;
        private IDisposable _soldierAddedSubscription;

        private readonly EntityId[] _registeredIds =
            new EntityId[4];

        private readonly bool[] _registered =
            new bool[4];

        public bool IsInitialized { get; private set; }

        public void Initialize(
            ISoldierWeaponAnimationRegistry registry,
            IEventSubscriber eventSubscriber)
        {
            if (IsInitialized)
            {
                return;
            }

            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            if (eventSubscriber == null)
            {
                throw new ArgumentNullException(nameof(eventSubscriber));
            }

            ValidateViews();

            _registry = registry;
            _soldierAddedSubscription =
                eventSubscriber.Subscribe<SoldierAddedEvent>(
                    OnSoldierAdded);

            IsInitialized = true;
        }

        private void ValidateViews()
        {
            if (soldierViews == null ||
                soldierViews.Length != 4)
            {
                throw new InvalidOperationException(
                    "Exactly four SoldierView references are required.");
            }

            for (int i = 0; i < soldierViews.Length; i++)
            {
                if (soldierViews[i] == null)
                {
                    throw new InvalidOperationException(
                        $"SoldierView at slot {i} is not assigned.");
                }
            }
        }

        private void OnSoldierAdded(
            SoldierAddedEvent evt)
        {
            int slot = evt.SlotIndex;

            if (slot < 0 ||
                slot >= soldierViews.Length)
            {
                return;
            }

            if (_registered[slot])
            {
                _registry.Unregister(
                    _registeredIds[slot]);
            }

            _registry.Register(
                evt.SoldierId,
                soldierViews[slot]);

            _registeredIds[slot] =
                evt.SoldierId;

            _registered[slot] =
                true;
        }

        private void OnDestroy()
        {
            _soldierAddedSubscription?.Dispose();

            if (_registry != null)
            {
                for (int i = 0; i < _registered.Length; i++)
                {
                    if (_registered[i])
                    {
                        _registry.Unregister(
                            _registeredIds[i]);
                    }
                }
            }

            IsInitialized = false;
        }
    }
}
