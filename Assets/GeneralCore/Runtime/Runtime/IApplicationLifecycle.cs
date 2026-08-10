namespace GeneralCore.Runtime
{
    public interface IApplicationLifecycle
    {
        bool IsFocused { get; }
        bool IsPaused { get; }
    }
}
