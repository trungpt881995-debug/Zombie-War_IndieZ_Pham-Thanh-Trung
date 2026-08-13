using GameplayCore.Entities; using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.Ports { public interface IBossTargetProvider { bool TryAcquireTarget(in BossPoint bossPosition,out BossTarget target); bool TryGetTarget(EntityId entityId,out BossTarget target); } }
