namespace GameplayCore.Movement
{
    public interface IMovable<TInput>
    {
        void Move(TInput input, float deltaTime);
    }
}
