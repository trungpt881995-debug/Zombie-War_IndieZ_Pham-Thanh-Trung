namespace ZombieWar.Features.Damage.View
{
    /// <summary>
    /// Null Object used by production when Damage has no direct presentation.
    /// </summary>
    public sealed class NullDamageView : IDamageView
    {
        public static readonly NullDamageView Instance = new NullDamageView();

        private NullDamageView()
        {
        }

        public void Render(in DamageViewState state)
        {
        }
    }
}
