namespace GeneralCore.PerformanceMemory
{
    public interface IPool<T>
    {
        T Rent();
        void Return(T item);
        int CountInactive { get; }
    }
}
