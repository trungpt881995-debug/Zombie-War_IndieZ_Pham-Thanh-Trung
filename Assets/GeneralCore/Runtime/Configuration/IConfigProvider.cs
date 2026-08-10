namespace GeneralCore.Configuration
{
    public interface IConfig { }

    public interface IConfigProvider
    {
        bool TryGet<TConfig>(out TConfig config) where TConfig : class, IConfig;
    }
}
