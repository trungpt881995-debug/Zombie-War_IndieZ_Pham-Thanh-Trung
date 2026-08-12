using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Weapon.Catalog;
using ZombieWar.Features.Weapon.Controller;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Model;
using ZombieWar.Features.Weapon.View;

namespace ZombieWar.Features.Weapon.Services
{
    public sealed class WeaponRuntime : IWeaponRuntime
    {
        private readonly IEventBus _events;
        private readonly IWeaponView _view;
        private IWeaponCatalog _catalog;
        private WeaponLoadoutModel _model;
        private WeaponSelectionController _controller;

        public bool IsInitialized => _controller != null;
        public bool GameplayEnabled => IsInitialized && _model.GameplayEnabled;
        public WeaponType CurrentWeapon => IsInitialized ? _model.CurrentWeapon : WeaponType.Pistol;
        public float CurrentTargetRange =>
            TryGetCurrentDefinition(out WeaponDefinition definition) ? definition.TargetRange : 0f;
        public WeaponCooldownSnapshot Cooldowns =>
            IsInitialized ? _model.SnapshotCooldowns() : default;

        public WeaponRuntime(IEventBus events, IWeaponView view)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Initialize(IWeaponCatalog catalog, WeaponType initialWeapon)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _catalog.Get(initialWeapon);
            _model = new WeaponLoadoutModel(_catalog, initialWeapon);
            _controller = new WeaponSelectionController(_model, _catalog, _view, _events);
        }

        public WeaponSelectionResult TrySelect(WeaponType type)
        {
            if (!IsInitialized)
                return WeaponSelectionResult.Rejected(WeaponType.Pistol, WeaponSelectionRejectReason.NotInitialized);
            return _controller.TrySelect(type);
        }

        public bool TryGetCurrentDefinition(out WeaponDefinition definition)
        {
            if (!IsInitialized) { definition = default; return false; }
            return _catalog.TryGet(_model.CurrentWeapon, out definition);
        }

        public bool TryGetDefinition(WeaponType type, out WeaponDefinition definition)
        {
            if (!IsInitialized) { definition = default; return false; }
            return _catalog.TryGet(type, out definition);
        }

        public void Tick(float deltaTime)
        {
            if (IsInitialized) _controller.Tick(deltaTime);
        }

        public void SetGameplayEnabled(bool enabled)
        {
            if (IsInitialized) _controller.SetGameplayEnabled(enabled);
        }

        public void ResetForGameLevel()
        {
            if (IsInitialized) _controller.ResetForGameLevel();
        }
    }
}
