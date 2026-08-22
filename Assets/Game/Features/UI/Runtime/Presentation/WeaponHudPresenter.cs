using ZombieWar.Features.UI.Domain;
using ZombieWar.Features.UI.Model;
using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Presentation
{
    public sealed class WeaponHudPresenter
    {
        private readonly GameplayHudModel _model;
        private IGameplayHudView _view;
        public WeaponHudPresenter(GameplayHudModel model) => _model = model;
        public void Bind(IGameplayHudView view)
        {
            _view = view;
            RenderAll();
        }
        public void Unbind() => _view = null;
        public void PresentSelected(UIWeaponId selected)
        {
            _model.SelectedWeapon = selected;
            RenderAll();
        }
        public void PresentWeapon(UIWeaponId id, float cooldown, bool interactable)
        {
            _model.SetWeapon(id, cooldown, interactable);
            Render(id);
        }
        private void RenderAll()
        {
            for (int i = 0; i < 6; i++) Render((UIWeaponId) i);
        }
        private void Render(UIWeaponId id) => _view?.SetWeaponState(id, id == _model.SelectedWeapon, _model.GetCooldown(id),
        _model.GetInteractable(id));
    }
}
