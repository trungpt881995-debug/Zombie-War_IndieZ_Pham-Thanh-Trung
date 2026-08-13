using System; using GeneralCore.Architecture; using ZombieWar.Features.GameState.Domain; using ZombieWar.Features.GameState.Events; using ZombieWar.Features.GameState.Services; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Services;
namespace ZombieWar.Integration.VFX.GameState
{
    public sealed class GameStateVFXBridge:IDisposable
    {
        private readonly IEventSubscriber _events; private readonly IGameStateRuntime _gameState; private readonly IVFXRuntime _vfx; private IDisposable _sub;
        public GameStateVFXBridge(IEventSubscriber events,IGameStateRuntime gameState,IVFXRuntime vfx){_events=events;_gameState=gameState;_vfx=vfx;}
        public void Start(){if(_sub!=null)return;_vfx.SetMode(Map(_gameState.State));_sub=_events.Subscribe<GameplayStateChangedEvent>(e=>_vfx.SetMode(Map(e.Current)));}
        public void Dispose(){_sub?.Dispose();_sub=null;}
        private static VFXGameplayMode Map(GameplayStateId s){switch(s){case GameplayStateId.Playing:return VFXGameplayMode.Playing;case GameplayStateId.Paused:return VFXGameplayMode.Suspended;case GameplayStateId.GameOver:case GameplayStateId.LevelComplete:case GameplayStateId.EndGame:return VFXGameplayMode.TerminalDrain;default:return VFXGameplayMode.Inactive;}}
    }
}
