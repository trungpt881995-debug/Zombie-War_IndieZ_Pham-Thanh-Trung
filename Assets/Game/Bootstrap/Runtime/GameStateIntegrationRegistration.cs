using System;
using VContainer.Unity;
using ZombieWar.Integration.GameState.GameFlow;
using ZombieWar.Integration.GameState.Level;
using ZombieWar.Integration.GameState.Runtime;
using ZombieWar.Integration.GameState.Soldier;

namespace ZombieWar.Bootstrap
{
    public sealed class GameStateIntegrationRegistration : IStartable, IDisposable
    {
        private readonly GameStateGameplayGateBridge _gates;
        private readonly SoldierDefeatGameStateBridge _soldierDefeat;
        private readonly LevelGameStateBridge _level;
        private readonly GameFlowGameStateBridge _flow;

        public GameStateIntegrationRegistration(
            GameStateGameplayGateBridge gates,
            SoldierDefeatGameStateBridge soldierDefeat,
            LevelGameStateBridge level,
            GameFlowGameStateBridge flow)
        {
            _gates = gates;
            _soldierDefeat = soldierDefeat;
            _level = level;
            _flow = flow;
        }

        public void Start()
        {
            // Gate startup and GameFlow synchronization are idempotent; the flow bridge also re-applies gates on every flow change.
            _gates.Start();
            _soldierDefeat.Start();
            _level.Start();
            _flow.Start();
        }

        public void Dispose()
        {
            _flow.Dispose();
            _level.Dispose();
            _soldierDefeat.Dispose();
            _gates.Dispose();
        }
    }
}
