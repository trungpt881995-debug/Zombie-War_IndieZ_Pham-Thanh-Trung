using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.Ports { public interface IBossAttackPort { bool TryAttack(in BossAttackRequest request); } }
