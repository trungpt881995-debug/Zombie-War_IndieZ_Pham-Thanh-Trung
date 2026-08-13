using System; using GeneralCore.Architecture; using VContainer.Unity; using ZombieWar.Features.Level.Commands; using ZombieWar.Features.Zombie.Events;
namespace ZombieWar.Integration.Level.Zombie
{
    public sealed class ZombieKillToLevelProgressAdapter:IStartable,IDisposable
    {
        private readonly IEventSubscriber _events; private readonly ICommandBus _commands; private IDisposable _subscription;
        public ZombieKillToLevelProgressAdapter(IEventSubscriber events,ICommandBus commands){_events=events??throw new ArgumentNullException(nameof(events));_commands=commands??throw new ArgumentNullException(nameof(commands));}
        public void Start(){_subscription=_events.Subscribe<ZombieKilledEvent>(OnKilled);} private void OnKilled(ZombieKilledEvent e){_commands.Send(new RegisterNormalZombieKillCommand());} public void Dispose(){_subscription?.Dispose();_subscription=null;}
    }
}
