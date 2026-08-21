using System; using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Ports;
namespace ZombieWar.Features.Spawn.Validation
{
    public sealed class SpawnPlacementValidator : ISpawnPlacementValidator
    {
        private readonly ISpawnVisibilityQuery _visibility;
        private readonly ISpawnGameplayBoundsQuery _bounds;
        private readonly ISpawnNavigationQuery _navigation;
        public SpawnPlacementValidator(ISpawnVisibilityQuery visibility,ISpawnGameplayBoundsQuery bounds,ISpawnNavigationQuery navigation)
        { 
            _visibility=visibility??throw new ArgumentNullException(nameof(visibility)); 
            _bounds=bounds??throw new ArgumentNullException(nameof(bounds)); 
            _navigation=navigation??throw new ArgumentNullException(nameof(navigation)); 
        }
        public SpawnPlacementResult Validate(in SpawnPoint candidate)
        {
            if(_visibility.IsVisible(in candidate)) 
            return SpawnPlacementResult.Rejected(SpawnRejectReason.InsideCamera);

            if(!_bounds.Contains(in candidate)) 
            return SpawnPlacementResult.Rejected(SpawnRejectReason.OutsideGameplayBounds);

            if(!_navigation.TryResolve(in candidate,out SpawnPoint resolved)) 
            return SpawnPlacementResult.Rejected(SpawnRejectReason.InvalidNavigation);
            
            return SpawnPlacementResult.Accepted(in resolved);
        }
    }
}
