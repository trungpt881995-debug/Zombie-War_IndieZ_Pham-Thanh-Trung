using System; using GeneralCore.Architecture; using ZombieWar.Features.Score.Events; using ZombieWar.Features.Score.Services; using ZombieWar.Features.UI.Presentation;
namespace ZombieWar.Integration.UI.Score
{
    public sealed class ScoreUIBridge:IDisposable
    { private readonly IScoreRuntime _runtime; private readonly ScorePresenter _presenter; private readonly IEventSubscriber _events; private IDisposable _sub;
      public ScoreUIBridge(IScoreRuntime runtime,ScorePresenter presenter,IEventSubscriber events){_runtime=runtime;_presenter=presenter;_events=events;}
      public void Start(){_presenter.Present(_runtime.TotalScore);_sub=_events.Subscribe<ScoreChangedEvent>(e=>_presenter.Present(e.CurrentTotal));} public void Dispose(){_sub?.Dispose();_sub=null;} }
}
