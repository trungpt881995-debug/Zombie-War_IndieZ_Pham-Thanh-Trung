using System; using GeneralCore.Architecture; using VContainer.Unity; using ZombieWar.Features.Boss.Commands; using ZombieWar.Features.Boss.Domain; using ZombieWar.Features.Level.Domain; using ZombieWar.Features.Level.Events; using ZombieWar.Features.Map.Services;
namespace ZombieWar.Integration.Boss.Level
{
    public sealed class LevelBossPhaseBridge:IStartable,IDisposable
    {
        private readonly IEventSubscriber _events;private readonly ICommandBus _commands;private readonly IMapRuntime _map;private IDisposable _subscription;
        public LevelBossPhaseBridge(IEventSubscriber events,ICommandBus commands,IMapRuntime map){_events=events??throw new ArgumentNullException(nameof(events));_commands=commands??throw new ArgumentNullException(nameof(commands));_map=map??throw new ArgumentNullException(nameof(map));}
        public void Start()=>_subscription=_events.Subscribe<BossPhaseStartedEvent>(OnBossPhase);
        private void OnBossPhase(BossPhaseStartedEvent e)
        {
            if(!_map.TryGetCurrentContext(out var context))return;if(!TryMap(e.RequiredBossObjectives,out BossSpawnSelection selection))return;var mp=context.BossSpawnPoint;var anchor=new BossPoint(mp.X,mp.Y,mp.Z);_commands.Send(new SpawnLevelBossesCommand(in selection,in anchor));
        }
        private static bool TryMap(LevelBossObjectiveId required,out BossSpawnSelection selection)
        {
            bool a=(required&LevelBossObjectiveId.BossA)!=0,b=(required&LevelBossObjectiveId.BossB)!=0,c=(required&LevelBossObjectiveId.BossC)!=0;int count=(a?1:0)+(b?1:0)+(c?1:0);
            if(count==1){selection=new BossSpawnSelection(a?BossId.BossA:b?BossId.BossB:BossId.BossC);return true;}if(count==2){BossId first=a?BossId.BossA:BossId.BossB;BossId second=c?BossId.BossC:BossId.BossB;if(first==second){selection=default;return false;}selection=new BossSpawnSelection(first,second);return true;}selection=default;return false;
        }
        public void Dispose(){_subscription?.Dispose();_subscription=null;}
    }
}
