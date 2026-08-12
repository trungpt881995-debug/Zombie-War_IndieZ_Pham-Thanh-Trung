namespace ZombieWar.Features.Soldier.Ports
{
    /// <summary>
    /// Temporary until Weapon Feature provides WeaponConfig.TargetRange.
    /// A zero range intentionally prevents normal target acquisition.
    /// </summary>
    public sealed class NullTargetRangeProvider : ITargetRangeProvider
    {
        public static readonly NullTargetRangeProvider Instance = new NullTargetRangeProvider();

        private NullTargetRangeProvider()
        {
        }

        public float CurrentTargetRange => 0f;
    }
}
