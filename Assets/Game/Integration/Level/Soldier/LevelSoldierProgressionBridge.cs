using System; using GeneralCore.Architecture; using VContainer.Unity; using ZombieWar.Features.Level.Domain; using ZombieWar.Features.Level.Events; using ZombieWar.Features.Soldier.Domain;
namespace ZombieWar.Integration.Level.Soldier
{
    public sealed class LevelSoldierProgressionBridge:IStartable,IDisposable,ILevelSoldierBinding
    {
        private readonly IEventSubscriber _events; private IDisposable _started,_changed; private ISoldierGroupRuntime _runtime; private SoldierGroupLevelId _desired=SoldierGroupLevelId.Level1; private bool _hasLevel;
        public LevelSoldierProgressionBridge(IEventSubscriber events){_events=events??throw new ArgumentNullException(nameof(events));}
        public void Start(){_started=_events.Subscribe<GameLevelStartedEvent>(OnStarted);_changed=_events.Subscribe<SoldierGroupLevelChangedEvent>(OnChanged);}
        public void Bind(ISoldierGroupRuntime runtime){_runtime=runtime??throw new ArgumentNullException(nameof(runtime));if(_hasLevel)ApplyDesired(reset:true);}
        public void Unbind(ISoldierGroupRuntime runtime){if(ReferenceEquals(_runtime,runtime))_runtime=null;}
        private void OnStarted(GameLevelStartedEvent e){_hasLevel=true;_desired=SoldierGroupLevelId.Level1;ApplyDesired(reset:true);}
        private void OnChanged(SoldierGroupLevelChangedEvent e){_hasLevel=true;_desired=e.Current;ApplyDesired(reset:false);}
        private void ApplyDesired(bool reset){if(_runtime==null)return;if(reset)_runtime.ResetForGameLevel();while((int)_runtime.Level<(int)_desired){var next=(SoldierGroupLevel)((int)_runtime.Level+1);if(!_runtime.TryAdvanceTo(next))break;}}
        public void Dispose(){_started?.Dispose();_changed?.Dispose();_started=_changed=null;_runtime=null;}
    }
}
