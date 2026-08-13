using System; using ZombieWar.Features.Boss.Domain; using ZombieWar.Features.Boss.Ports;
namespace ZombieWar.Features.Boss.Strategies
{
    public sealed class BasicMeleeBossAttackStrategy:IBossAttackStrategy
    {
        private readonly IBossAttackPort _port; public BasicMeleeBossAttackStrategy(IBossAttackPort port){_port=port??throw new ArgumentNullException(nameof(port));}
        public bool TryExecute(in BossAttackRequest request)=>_port.TryAttack(in request);
    }
}
