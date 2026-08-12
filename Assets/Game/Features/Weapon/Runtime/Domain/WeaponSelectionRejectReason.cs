namespace ZombieWar.Features.Weapon.Domain
{
    public enum WeaponSelectionRejectReason
    {
        None = 0,
        NotInitialized = 1,
        GameplayDisabled = 2,
        AlreadySelected = 3,
        OnCooldown = 4,
        UnknownWeapon = 5
    }
}
