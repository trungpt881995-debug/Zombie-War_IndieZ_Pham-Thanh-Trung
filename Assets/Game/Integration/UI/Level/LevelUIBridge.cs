using System; using GeneralCore.Architecture; using ZombieWar.Features.Level.Events; using ZombieWar.Features.Level.Services; using ZombieWar.Features.UI.Presentation;
namespace ZombieWar.Integration.UI.Level
{
    public sealed class LevelUIBridge:IDisposable
    { private readonly ILevelRuntime _runtime; private readonly LevelPresenter _presenter; private readonly IEventSubscriber _events; private IDisposable _started,_group;
      public LevelUIBridge(ILevelRuntime runtime,LevelPresenter presenter,IEventSubscriber events){_runtime=runtime;_presenter=presenter;_events=events;}
      public void Start(){if(_runtime.IsInitialized){var s=_runtime.Snapshot();_presenter.Present((int)s.GameLevel,(int)s.SoldierGroupLevel);} _started=_events.Subscribe<GameLevelStartedEvent>(e=>_presenter.Present((int)e.GameLevel,(int)e.SoldierGroupLevel)); _group=_events.Subscribe<SoldierGroupLevelChangedEvent>(e=>_presenter.Present((int)e.GameLevel,(int)e.Current));}
      public void Dispose(){_started?.Dispose();_group?.Dispose();_started=_group=null;} }
}
