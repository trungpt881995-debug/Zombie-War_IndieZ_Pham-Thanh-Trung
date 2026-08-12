using GeneralCore.Architecture;

namespace ZombieWar.Features.Weapon.View
{
    public interface IWeaponView : IView
    {
        void Render(in WeaponViewState state);
    }
}
