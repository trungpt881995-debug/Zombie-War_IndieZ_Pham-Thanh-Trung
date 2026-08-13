using GameplayCore.Entities; using ZombieWar.Features.VFX.Ports;
namespace ZombieWar.Integration.VFX.Soldier { public interface ISoldierVFXAnchorBinding { void Bind(EntityId groupId,IVFXAnchor anchor); void Unbind(EntityId groupId); } }
