using GeneralCore.Architecture;

namespace ZombieWar.Features.Targeting.View
{
    public interface ITargetingView : IView
    {
        void Render(in TargetingViewState state);
    }
}
