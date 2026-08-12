using GameplayCore.Targeting;

namespace ZombieWar.Features.Targeting.Domain
{
    /// <summary>
    /// Game-facing target port. Zombie/Boss adapters implement this contract;
    /// Targeting never depends on their concrete controllers or Unity GameObjects.
    /// </summary>
    public interface ITargetCandidate : ITargetable
    {
        TargetPoint Position { get; }
    }
}
