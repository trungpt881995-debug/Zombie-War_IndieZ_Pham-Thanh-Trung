using GeneralCore.Architecture;

namespace ZombieWar.Features.Health.View
{
    /// <summary>
    /// Presentation contract only. Concrete Unity HP bars belong to the UI Feature.
    /// </summary>
    public interface IHealthView : IView
    {
        void Render(in HealthViewState state);
    }
}
