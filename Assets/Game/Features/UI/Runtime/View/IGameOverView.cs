using System;

namespace ZombieWar.Features.UI.View
{
    public interface IGameOverView : IUIScreenView
    {
        event Action ReplayClicked;
        event Action MenuClicked;
    }
}
