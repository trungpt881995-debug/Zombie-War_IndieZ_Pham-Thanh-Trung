namespace ZombieWar.Features.Health.View
{
    /// <summary>
    /// Null Object used by owners that have no visible health presentation.
    /// </summary>
    public sealed class NullHealthView : IHealthView
    {
        public static readonly NullHealthView Instance = new NullHealthView();

        private NullHealthView()
        {
        }

        public void Render(in HealthViewState state)
        {
        }
    }
}
