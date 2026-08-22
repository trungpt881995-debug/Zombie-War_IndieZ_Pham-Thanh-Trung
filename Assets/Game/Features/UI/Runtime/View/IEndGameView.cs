using System;

namespace ZombieWar.Features.UI.View
{
    public interface IEndGameView : IUIScreenView
    {
        event Action ReplayClicked;
        event Action MenuClicked;
        void SetFinalScore(long score);
        void SetReplayVisible(bool visible);
    }
}
