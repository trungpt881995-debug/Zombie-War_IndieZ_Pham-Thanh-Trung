using GameplayCore.Entities; using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.Ports
{
    public interface IBossFeedbackPort { void OnSpawn(BossId bossId,EntityId entityId,in BossPoint position); void OnHit(BossId bossId,EntityId entityId,in BossPoint position); void OnDeath(BossId bossId,EntityId entityId,in BossPoint position); }
    public sealed class NullBossFeedbackPort:IBossFeedbackPort { public static readonly NullBossFeedbackPort Instance=new NullBossFeedbackPort(); private NullBossFeedbackPort(){} public void OnSpawn(BossId b,EntityId e,in BossPoint p){} public void OnHit(BossId b,EntityId e,in BossPoint p){} public void OnDeath(BossId b,EntityId e,in BossPoint p){} }
}
