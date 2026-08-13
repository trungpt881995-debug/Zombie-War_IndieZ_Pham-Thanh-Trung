using System; using ZombieWar.Features.UI.Domain; using ZombieWar.Features.UI.Ports; using ZombieWar.GameFlow.Controller;
namespace ZombieWar.Integration.UI.GameFlow
{
    public sealed class GameFlowUIActionPort:IUIFlowActionPort,IGameFlowUIActionContext
    { private readonly GameFlowController _flow; public UIFlowAction PendingAction{get;private set;}
      public GameFlowUIActionPort(GameFlowController flow)=>_flow=flow??throw new ArgumentNullException(nameof(flow));
      public void Play(){PendingAction=UIFlowAction.Play;_flow.BeginLoading();} public void Replay(){PendingAction=UIFlowAction.Replay;_flow.BeginLoading();} public void Next(){PendingAction=UIFlowAction.Next;_flow.BeginLoading();}
      public void Menu(){PendingAction=UIFlowAction.Menu;_flow.GoToMainMenu();}
      public UIFlowAction Consume(){var value=PendingAction;PendingAction=UIFlowAction.None;return value;}
    }
}
