namespace ZombieWar.Features.Soldier.View
{
    /// <summary>
    /// Presentation-only contract used by Weapon integration after a real shot succeeds.
    /// It deliberately contains no Weapon domain types so Soldier remains feature-isolated.
    /// </summary>
    public interface ISoldierWeaponAnimationView
    {
        void PlayShoot();
    }
}
