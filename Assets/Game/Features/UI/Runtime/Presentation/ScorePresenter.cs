using ZombieWar.Features.UI.Model;
using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Presentation
{
    public sealed class ScorePresenter
    {
        private readonly GameplayHudModel _model;
        private IGameplayHudView _hud;
        private IEndGameView _end;
        public ScorePresenter(GameplayHudModel model) => _model = model;
        public long Score => _model.Score;
        public void Bind(IGameplayHudView hud, IEndGameView end)
        {
            _hud = hud;
            _end = end;
            Render();
        }
        public void Unbind()
        {
            _hud = null;
            _end = null;
        }
        public void Present(long score)
        {
            _model.Score = score < 0 ? 0 : score;
            Render();
        }
        private void Render()
        {
            _hud?.SetScore(_model.Score);
            _end?.SetFinalScore(_model.Score);
        }
    }
}
