namespace ZombieWar.Features.Map.Domain
{
    public enum MapLoadFailureReason
    {
        None = 0,
        NotInitialized = 1,
        InvalidMapId = 2,
        MissingDefinition = 3,
        LoaderFailed = 4,
        InvalidMapInstance = 5,
        InvalidMapView = 6,
        InvalidRuntimeContext = 7,
        Cancelled = 8
    }
}
