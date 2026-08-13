using System; using ZombieWar.Features.UI.Ports; using ZombieWar.Features.UI.Utilities; using ZombieWar.Features.UI.View;
namespace ZombieWar.Features.UI.Controller
{
    public sealed class PauseController
    { private readonly IUIFlowActionPort _flow; private readonly IGameplayPausePort _pause; private readonly SingleExecutionGuard _nav=new SingleExecutionGuard(); private IPauseView _view;
      public PauseController(IUIFlowActionPort flow,IGameplayPausePort pause){_flow=flow??throw new ArgumentNullException(nameof(flow));_pause=pause??throw new ArgumentNullException(nameof(pause));}
      public void Bind(IPauseView v){Unbind();_view=v??throw new ArgumentNullException(nameof(v));_view.ReplayClicked+=Replay;_view.ContinueClicked+=Continue;_view.MenuClicked+=Menu;}
      public void Unbind(){if(_view!=null){_view.ReplayClicked-=Replay;_view.ContinueClicked-=Continue;_view.MenuClicked-=Menu;}_view=null;} public void Reset()=>_nav.Reset();
      private void Replay(){if(_nav.TryEnter())_flow.Replay();} private void Continue()=>_pause.Resume(); private void Menu(){if(_nav.TryEnter())_flow.Menu();} }
}
