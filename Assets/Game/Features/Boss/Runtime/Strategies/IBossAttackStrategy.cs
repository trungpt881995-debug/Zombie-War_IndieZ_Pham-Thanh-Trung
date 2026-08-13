using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.Strategies { public interface IBossAttackStrategy { bool TryExecute(in BossAttackRequest request); } }
