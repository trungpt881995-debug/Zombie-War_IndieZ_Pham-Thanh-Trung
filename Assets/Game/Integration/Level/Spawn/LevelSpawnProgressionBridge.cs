using System; using GeneralCore.Architecture; using VContainer.Unity; using ZombieWar.Features.Level.Domain; using ZombieWar.Features.Level.Events; using ZombieWar.Features.Spawn.Commands; using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Integration.Level.Spawn
{
    public sealed class LevelSpawnProgressionBridge:IStartable,IDisposable
    {
        private readonly IEventSubscriber _events; private readonly ICommandBus _commands; private IDisposable _started,_changed,_boss;
        public LevelSpawnProgressionBridge(IEventSubscriber events,ICommandBus commands){_events=events??throw new ArgumentNullException(nameof(events));_commands=commands??throw new ArgumentNullException(nameof(commands));}
        public void Start(){_started=_events.Subscribe<GameLevelStartedEvent>(OnStarted);_changed=_events.Subscribe<SoldierGroupLevelChangedEvent>(OnChanged);_boss=_events.Subscribe<BossPhaseStartedEvent>(OnBoss);}
        private void OnStarted(GameLevelStartedEvent e)=>SetDifficulty(e.GameLevel,e.SoldierGroupLevel);
        private void OnChanged(SoldierGroupLevelChangedEvent e)=>SetDifficulty(e.GameLevel,e.Current);
        private void OnBoss(BossPhaseStartedEvent e)=>_commands.Send(new StopZombieSpawningCommand(SpawnStopReason.BossPhase));
        private void SetDifficulty(GameLevelId gameLevel,SoldierGroupLevelId soldierLevel){var key=new SpawnDifficultyKey((int)gameLevel,(int)soldierLevel);_commands.Send(new SetSpawnDifficultyCommand(in key));}
        public void Dispose(){_started?.Dispose();_changed?.Dispose();_boss?.Dispose();_started=_changed=_boss=null;}
    }
}
