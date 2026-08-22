using ZombieWar.Features.UI.Model;
using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Presentation
{
    public sealed class LevelPresenter
    {
        private readonly GameplayHudModel _model;
        private IGameplayHudView _view;
        public LevelPresenter(GameplayHudModel model) => _model = model;
        public void Bind(IGameplayHudView view)
        {
            _view = view;
            Render();
        }
        public void Unbind() => _view = null;
        public void Present(int gameLevel, int soldierGroupLevel)
        {
            _model.GameLevel = gameLevel < 0 ? 0 : gameLevel;
            _model.SoldierGroupLevel = soldierGroupLevel < 0 ? 0 : soldierGroupLevel;
            Render();
        }
        private void Render()
        {
            _view?.SetGameLevel(_model.GameLevel);
            _view?.SetSoldierGroupLevel(_model.SoldierGroupLevel);
        }
    }
}
