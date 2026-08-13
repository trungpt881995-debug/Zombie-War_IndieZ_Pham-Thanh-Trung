using System; using VContainer.Unity; using ZombieWar.Integration.UI.GameFlow; using ZombieWar.Integration.UI.Health; using ZombieWar.Integration.UI.Level; using ZombieWar.Integration.UI.Score; using ZombieWar.Integration.UI.Weapon;
namespace ZombieWar.Bootstrap
{
    public sealed class UIIntegrationRegistration:IStartable,ITickable,IDisposable
    { private readonly GameFlowUIBridge _flow; private readonly ScoreUIBridge _score; private readonly LevelUIBridge _level; private readonly IUIHealthBinding _health; private readonly WeaponUIBridge _weapon;
      public UIIntegrationRegistration(GameFlowUIBridge flow,ScoreUIBridge score,LevelUIBridge level,IUIHealthBinding health,WeaponUIBridge weapon){_flow=flow;_score=score;_level=level;_health=health;_weapon=weapon;}
      public void Start(){_flow.Start();_score.Start();_level.Start();_health.Start();_weapon.Start();} public void Tick()=>_weapon.Tick(); public void Dispose(){_weapon.Dispose();_health.Dispose();_level.Dispose();_score.Dispose();_flow.Dispose();} }
}
