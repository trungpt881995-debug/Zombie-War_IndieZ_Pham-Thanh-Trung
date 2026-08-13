using System;
namespace ZombieWar.Features.UI.View { public interface IMainMenuView:IUIScreenView { event Action PlayClicked; void SetTitle(string title); } }
