using GeneralCore.Architecture;

namespace ZombieWar.Features.Damage.View
{
    /// <summary>
    /// Presentation port only. Concrete UI/VFX must live in their own Feature.
    /// </summary>
    public interface IDamageView : IView
    {
        void Render(in DamageViewState state);
    }
}
