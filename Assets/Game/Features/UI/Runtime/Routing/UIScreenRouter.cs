using System;
using ZombieWar.Features.UI.Domain;
using ZombieWar.Features.UI.Model;
using ZombieWar.Features.UI.View;
namespace ZombieWar.Features.UI.Routing
{
    public sealed class UIScreenRouter:IUIScreenRouter
    {
        private readonly UIScreenModel _model; private IUIRootView _root;
        public UIScreenId Current=>_model.Current; public event Action<UIScreenId> ScreenChanged;
        public UIScreenRouter(UIScreenModel model){_model=model??throw new ArgumentNullException(nameof(model));_model.Changed+=s=>ScreenChanged?.Invoke(s);}
        public void Bind(IUIRootView root){_root=root??throw new ArgumentNullException(nameof(root)); Apply(_model.Current);}
        public void Unbind(IUIRootView root){if(ReferenceEquals(_root,root))_root=null;}
        public bool Show(UIScreenId screen){if(screen<UIScreenId.None||screen>UIScreenId.EndGame)throw new ArgumentOutOfRangeException(nameof(screen)); bool changed=_model.Set(screen); Apply(screen); return changed;}
        public void HideAll()=>Show(UIScreenId.None);
        private void Apply(UIScreenId current)
        {
            if(_root==null)return;
            Set(_root.MainMenu,current==UIScreenId.MainMenu); Set(_root.Gameplay,current==UIScreenId.Gameplay);
            Set(_root.Pause,current==UIScreenId.Pause); Set(_root.LevelComplete,current==UIScreenId.LevelComplete);
            Set(_root.GameOver,current==UIScreenId.GameOver); Set(_root.EndGame,current==UIScreenId.EndGame);
        }
        private static void Set(IUIScreenView view,bool visible){view?.SetVisible(visible);}
    }
}
