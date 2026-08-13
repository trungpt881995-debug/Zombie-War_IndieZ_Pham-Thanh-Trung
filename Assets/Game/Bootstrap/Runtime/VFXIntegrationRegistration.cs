using System; using VContainer.Unity; using ZombieWar.Integration.VFX.GameState; using ZombieWar.Integration.VFX.Soldier;
namespace ZombieWar.Bootstrap
{
    public sealed class VFXIntegrationRegistration:IStartable,IDisposable
    {
        private readonly GameStateVFXBridge _state; private readonly SoldierDamageVFXBridge _soldier;
        public VFXIntegrationRegistration(GameStateVFXBridge state,SoldierDamageVFXBridge soldier){_state=state;_soldier=soldier;}
        public void Start(){_state.Start();_soldier.Start();} public void Dispose(){_soldier.Dispose();_state.Dispose();}
    }
}
