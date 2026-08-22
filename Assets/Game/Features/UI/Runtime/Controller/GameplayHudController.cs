using System;
using ZombieWar.Features.UI.Domain;
using ZombieWar.Features.UI.Ports;
using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Controller
{
    public sealed class GameplayHudController
    {
        private readonly IGameplayPausePort _pause;
        private readonly IWeaponSelectionPort _weapons;
        private IGameplayHudView _view;
        public GameplayHudController(IGameplayPausePort pause, IWeaponSelectionPort weapons)
        {
            _pause = pause ?? throw new ArgumentNullException(nameof(pause));
            _weapons = weapons ?? throw new ArgumentNullException(nameof(weapons));
        }
        public void Bind(IGameplayHudView v)
        {
            Unbind();
            _view = v ?? throw new ArgumentNullException(nameof(v));
            _view.PauseClicked += OnPause;
            _view.WeaponClicked += OnWeapon;
        }
        public void Unbind()
        {
            if (_view != null)
            {
                _view.PauseClicked -= OnPause;
                _view.WeaponClicked -= OnWeapon;
            }
            _view = null;
        }
        private void OnPause() => _pause.Pause();
        private void OnWeapon(UIWeaponId id) => _weapons.Select(id);
    }
}
