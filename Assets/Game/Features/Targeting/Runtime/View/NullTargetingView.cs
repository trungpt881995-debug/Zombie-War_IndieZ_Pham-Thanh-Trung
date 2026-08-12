namespace ZombieWar.Features.Targeting.View
{
    /// <summary>
    /// Null Object Pattern: production targeting does not require a visual view.
    /// </summary>
    public sealed class NullTargetingView : ITargetingView
    {
        public static readonly NullTargetingView Instance = new NullTargetingView();

        private NullTargetingView()
        {
        }

        public void Render(in TargetingViewState state)
        {
        }
    }
}
