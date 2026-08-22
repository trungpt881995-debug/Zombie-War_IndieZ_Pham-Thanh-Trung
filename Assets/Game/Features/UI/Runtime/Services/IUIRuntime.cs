using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Services
{
    public interface IUIRuntime
    {
        bool IsBound
        {
            get;
        }
        void Bind(IUIRootView root);
        void Unbind(IUIRootView root);
    }
}
