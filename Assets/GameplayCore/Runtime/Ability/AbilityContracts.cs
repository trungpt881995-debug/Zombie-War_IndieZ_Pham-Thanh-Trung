namespace GameplayCore.Ability
{
    public interface IAbility
    {
        bool CanExecute { get; }
        void Execute();
    }
}
