using VContainer.Unity;
using ZombieWar.Features.GameState.Services;

namespace ZombieWar.Bootstrap
{
    public sealed class GameStateBootstrapRegistration : IStartable
    {
        private readonly IGameStateRuntimeConfigurator _configurator;
        public GameStateBootstrapRegistration(IGameStateRuntimeConfigurator configurator) => _configurator = configurator;
        public void Start() => _configurator.Initialize();
    }
}
