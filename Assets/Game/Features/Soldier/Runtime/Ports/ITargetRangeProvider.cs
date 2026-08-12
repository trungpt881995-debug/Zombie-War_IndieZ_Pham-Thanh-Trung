namespace ZombieWar.Features.Soldier.Ports
{
    /// <summary>
    /// Weapon-owned TargetRange enters Soldier through this narrow port.
    /// Soldier never owns Weapon balance.
    /// </summary>
    public interface ITargetRangeProvider
    {
        float CurrentTargetRange { get; }
    }
}
