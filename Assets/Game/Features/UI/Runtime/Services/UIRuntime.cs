using System;
using ZombieWar.Features.UI.Controller; using ZombieWar.Features.UI.Domain; using ZombieWar.Features.UI.Presentation; using ZombieWar.Features.UI.Routing; using ZombieWar.Features.UI.View;
namespace ZombieWar.Features.UI.Services
{
    public sealed class UIRuntime:IUIRuntime
    {
        private readonly IUIScreenRouter _router; private readonly MainMenuController _main; private readonly GameplayHudController _gameplay; private readonly PauseController _pause;
        private readonly LevelCompleteController _complete; private readonly GameOverController _over; private readonly EndGameController _end;
        private readonly ScorePresenter _score; private readonly HealthPresenter _health; private readonly LevelPresenter _level; private readonly WeaponHudPresenter _weapon; private IUIRootView _root;
        public bool IsBound=>_root!=null;
        public UIRuntime(IUIScreenRouter router,MainMenuController main,GameplayHudController gameplay,PauseController pause,LevelCompleteController complete,GameOverController over,EndGameController end,ScorePresenter score,HealthPresenter health,LevelPresenter level,WeaponHudPresenter weapon)
        { _router=router;_main=main;_gameplay=gameplay;_pause=pause;_complete=complete;_over=over;_end=end;_score=score;_health=health;_level=level;_weapon=weapon;_router.ScreenChanged+=OnScreenChanged; }
        public void Bind(IUIRootView root){if(root==null)throw new ArgumentNullException(nameof(root));if(_root!=null)Unbind(_root);_root=root;_router.Bind(root);_main.Bind(root.MainMenu);_gameplay.Bind(root.Gameplay);_pause.Bind(root.Pause);_complete.Bind(root.LevelComplete);_over.Bind(root.GameOver);_end.Bind(root.EndGame);_score.Bind(root.Gameplay,root.EndGame);_health.Bind(root.Gameplay);_level.Bind(root.Gameplay);_weapon.Bind(root.Gameplay);OnScreenChanged(_router.Current);}
        public void Unbind(IUIRootView root){if(!ReferenceEquals(_root,root))return;_main.Unbind();_gameplay.Unbind();_pause.Unbind();_complete.Unbind();_over.Unbind();_end.Unbind();_score.Unbind();_health.Unbind();_level.Unbind();_weapon.Unbind();_router.Unbind(root);_root=null;}
        private void OnScreenChanged(UIScreenId id){switch(id){case UIScreenId.MainMenu:_main.Reset();break;case UIScreenId.Pause:_pause.Reset();break;case UIScreenId.LevelComplete:_complete.Reset();break;case UIScreenId.GameOver:_over.Reset();break;case UIScreenId.EndGame:_end.Reset();break;}}
    }
}
