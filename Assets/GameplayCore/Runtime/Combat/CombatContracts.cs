using GameplayCore.Entities;

namespace GameplayCore.Combat
{
    public readonly struct CombatContext
    {
        public EntityId Source { get; }
        public EntityId Target { get; }

        public CombatContext(EntityId source, EntityId target)
        {
            Source = source;
            Target = target;
        }
    }

    public interface ICombatAction<in TContext>
    {
        bool CanExecute(TContext context);
        void Execute(TContext context);
    }
}
