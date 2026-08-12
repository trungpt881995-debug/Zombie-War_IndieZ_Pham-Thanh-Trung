namespace ZombieWar.Features.Weapon.Domain
{
    public readonly struct WeaponSelectionResult
    {
        public bool Accepted { get; }
        public WeaponType Previous { get; }
        public WeaponType Current { get; }
        public WeaponSelectionRejectReason RejectReason { get; }

        private WeaponSelectionResult(
            bool accepted,
            WeaponType previous,
            WeaponType current,
            WeaponSelectionRejectReason reason)
        {
            Accepted = accepted;
            Previous = previous;
            Current = current;
            RejectReason = reason;
        }

        public static WeaponSelectionResult Success(
            WeaponType previous,
            WeaponType current) =>
            new WeaponSelectionResult(true, previous, current, WeaponSelectionRejectReason.None);

        public static WeaponSelectionResult Rejected(
            WeaponType current,
            WeaponSelectionRejectReason reason) =>
            new WeaponSelectionResult(false, current, current, reason);
    }
}
