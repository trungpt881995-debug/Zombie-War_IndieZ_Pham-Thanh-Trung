using System; using GeneralCore.Architecture; using VContainer.Unity; using ZombieWar.Features.Boss.Domain; using ZombieWar.Features.Boss.Events; using ZombieWar.Features.Level.Commands; using ZombieWar.Features.Level.Domain;
namespace ZombieWar.Integration.Boss.Level
{
    public sealed class BossDeathToLevelBridge:IStartable,IDisposable
    {
        private readonly IEventSubscriber _events;private readonly ICommandBus _commands;private IDisposable _subscription;public BossDeathToLevelBridge(IEventSubscriber events,ICommandBus commands){_events=events??throw new ArgumentNullException(nameof(events));_commands=commands??throw new ArgumentNullException(nameof(commands));}
        public void Start()=>_subscription=_events.Subscribe<BossDefeatedEvent>(OnDefeated);
        private void OnDefeated(BossDefeatedEvent e){LevelBossObjectiveId id=e.BossId==BossId.BossA?LevelBossObjectiveId.BossA:e.BossId==BossId.BossB?LevelBossObjectiveId.BossB:e.BossId==BossId.BossC?LevelBossObjectiveId.BossC:LevelBossObjectiveId.None;if(id!=LevelBossObjectiveId.None)_commands.Send(new RegisterBossDefeatedCommand(id));}
        public void Dispose(){_subscription?.Dispose();_subscription=null;}
    }
}
