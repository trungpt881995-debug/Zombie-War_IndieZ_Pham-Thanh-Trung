using GameplayCore.Entities;
namespace ZombieWar.Features.Boss.Ports { public interface IBossTargetRegistrationPort { void Register(EntityId entityId); void Unregister(EntityId entityId); } }
