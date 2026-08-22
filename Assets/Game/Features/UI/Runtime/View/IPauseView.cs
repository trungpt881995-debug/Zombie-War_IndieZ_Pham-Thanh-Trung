using System;

namespace ZombieWar.Features.UI.View
{
    public interface IPauseView : IUIScreenView
    {
        event Action ReplayClicked;
        event Action ContinueClicked;
        event Action MenuClicked;
    }
}
