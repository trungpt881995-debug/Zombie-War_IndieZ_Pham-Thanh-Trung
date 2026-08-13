namespace ZombieWar.Features.GameState.Services
{
    public interface IGameStateRuntimeConfigurator
    {
        void Initialize();
        void Shutdown();
    }
}
