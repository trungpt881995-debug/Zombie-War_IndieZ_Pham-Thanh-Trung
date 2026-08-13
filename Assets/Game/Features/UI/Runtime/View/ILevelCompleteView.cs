using System;
namespace ZombieWar.Features.UI.View { public interface ILevelCompleteView:IUIScreenView { event Action ReplayClicked; event Action NextClicked; event Action MenuClicked; } }
