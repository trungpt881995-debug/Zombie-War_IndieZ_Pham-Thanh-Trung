using ZombieWar.Features.Soldier.Domain;

namespace ZombieWar.Features.Soldier.View
{
    /// <summary>
    /// Optional presentation capability for per-Soldier body facing.
    /// Kept separate from ISoldierView so existing tests/adapters implementing
    /// the older view contract remain source-compatible.
    /// </summary>
    public interface ISoldierFacingView
    {
        SoldierDirection Forward { get; }

        void SetBodyFacing(
            in SoldierDirection direction,
            float rotationDegreesPerSecond,
            float deltaTime);
    }
}
