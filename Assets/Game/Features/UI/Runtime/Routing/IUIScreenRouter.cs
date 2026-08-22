using System;
using ZombieWar.Features.UI.Domain;
using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Routing
{
    public interface IUIScreenRouter
    {
        UIScreenId Current
        {
            get;
        }
        event Action < UIScreenId > ScreenChanged;
        void Bind(IUIRootView root);
        void Unbind(IUIRootView root);
        bool Show(UIScreenId screen);
        void HideAll();
    }
}
