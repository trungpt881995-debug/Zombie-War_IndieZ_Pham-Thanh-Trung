namespace ZombieWar.Features.Weapon.View
{
    public sealed class NullWeaponView : IWeaponView
    {
        public static readonly NullWeaponView Instance = new NullWeaponView();
        private NullWeaponView() { }
        public void Render(in WeaponViewState state) { }
    }
}
