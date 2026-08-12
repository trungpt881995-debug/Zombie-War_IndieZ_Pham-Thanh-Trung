using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Weapon.Catalog;
using ZombieWar.Features.Weapon.Domain;
using ZombieWar.Features.Weapon.Events;
using ZombieWar.Features.Weapon.Model;
using ZombieWar.Features.Weapon.View;

namespace ZombieWar.Features.Weapon.Controller
{
    public sealed class WeaponSelectionController : IController
    {
        private readonly WeaponLoadoutModel _model;
        private readonly IWeaponCatalog _catalog;
        private readonly IWeaponView _view;
        private readonly IEventBus _events;

        public WeaponSelectionController(
            WeaponLoadoutModel model,
            IWeaponCatalog catalog,
            IWeaponView view,
            IEventBus events)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _events = events ?? throw new ArgumentNullException(nameof(events));
            Render();
        }

        public WeaponSelectionResult TrySelect(WeaponType type)
        {
            WeaponSelectionResult result = _model.TrySelect(type);
            if (!result.Accepted) return result;

            WeaponDefinition previous = _catalog.Get(result.Previous);
            if (previous.SelectionCooldown > 0f)
                _events.Publish(new WeaponCooldownStartedEvent(result.Previous, previous.SelectionCooldown));
            _events.Publish(new WeaponSelectedEvent(result.Previous, result.Current));
            Render();
            return result;
        }

        public void Tick(float deltaTime)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0f) return;
            for (int i = 0; i < WeaponCatalog.RequiredWeaponCount; i++)
            {
                WeaponType type = (WeaponType)i;
                if (_model.TickCooldown(type, deltaTime))
                    _events.Publish(new WeaponCooldownReadyEvent(type));
            }
        }

        public void SetGameplayEnabled(bool enabled)
        {
            _model.SetGameplayEnabled(enabled);
            Render();
        }

        public void ResetForGameLevel()
        {
            _model.ResetForGameLevel();
            Render();
        }

        private void Render()
        {
            var state = new WeaponViewState(_model.CurrentWeapon, _model.GameplayEnabled);
            _view.Render(in state);
        }
    }
}
