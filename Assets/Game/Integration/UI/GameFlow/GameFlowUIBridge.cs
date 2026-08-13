using System; using ZombieWar.Features.UI.Domain; using ZombieWar.Features.UI.Routing; using ZombieWar.GameFlow.Domain; using ZombieWar.GameFlow.Model;
namespace ZombieWar.Integration.UI.GameFlow
{
    public sealed class GameFlowUIBridge:IDisposable
    { private readonly GameFlowModel _model; private readonly IUIScreenRouter _router; private bool _started;
      public GameFlowUIBridge(GameFlowModel model,IUIScreenRouter router){_model=model??throw new ArgumentNullException(nameof(model));_router=router??throw new ArgumentNullException(nameof(router));}
      public void Start(){if(_started)return;_started=true;_model.StateChanged+=OnState;OnState(_model.CurrentState);} public void Dispose(){if(!_started)return;_started=false;_model.StateChanged-=OnState;}
      private void OnState(GameFlowStateId s){switch(s){case GameFlowStateId.MainMenu:_router.Show(UIScreenId.MainMenu);break;case GameFlowStateId.Gameplay:_router.Show(UIScreenId.Gameplay);break;case GameFlowStateId.Paused:_router.Show(UIScreenId.Pause);break;case GameFlowStateId.LevelComplete:_router.Show(UIScreenId.LevelComplete);break;case GameFlowStateId.GameOver:_router.Show(UIScreenId.GameOver);break;case GameFlowStateId.EndGame:_router.Show(UIScreenId.EndGame);break;default:_router.HideAll();break;}}
    }
}
