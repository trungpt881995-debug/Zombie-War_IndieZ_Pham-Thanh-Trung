namespace GeneralCore.Platform
{
    public interface IPlatformService
    {
        string PlatformName { get; }
        string DeviceId { get; }
    }
}
