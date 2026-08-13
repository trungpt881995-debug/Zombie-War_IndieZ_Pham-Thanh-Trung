using GameplayCore.Entities;
using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Integration.GameState.Soldier
{
    public interface IGameStateSoldierBinding
    {
        bool IsBound { get; }
        EntityId GroupId { get; }
        void Bind(ISoldierGroupRuntime runtime);
        void Unbind(ISoldierGroupRuntime runtime);
    }
}
