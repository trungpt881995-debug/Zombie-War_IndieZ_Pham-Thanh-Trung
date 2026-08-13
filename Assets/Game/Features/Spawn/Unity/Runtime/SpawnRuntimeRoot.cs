using System; using UnityEngine; using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Ports; using ZombieWar.Features.Spawn.Services; using ZombieWar.Features.Spawn.Strategies; using ZombieWar.Features.Spawn.Unity.Config;
namespace ZombieWar.Features.Spawn.Unity.Runtime
{
    public sealed class SpawnRuntimeRoot : MonoBehaviour
    {
        [SerializeField] private SpawnConfig config;
        [SerializeField] private MonoBehaviour visibilityQueryBehaviour;
        [SerializeField] private MonoBehaviour navigationQueryBehaviour;
        [SerializeField] private MonoBehaviour zombieSpawnPortBehaviour;
        [SerializeField] private MonoBehaviour zombiePopulationQueryBehaviour;
        private ISpawnRuntime _runtime; private ISpawnRuntimeConfigurator _configurator;
        public bool IsInitialized=>_runtime!=null&&_runtime.IsInitialized; public ISpawnRuntime Runtime=>_runtime;
        public void Initialize(ISpawnRuntime runtime,ISpawnRuntimeConfigurator configurator,ISpawnRandom random,ISpawnSectorProvider sectorProvider,ISpawnGameplayBoundsQuery gameplayBoundsQuery)
        {
            if(IsInitialized)return; if(runtime==null)throw new ArgumentNullException(nameof(runtime)); if(configurator==null)throw new ArgumentNullException(nameof(configurator)); if(random==null)throw new ArgumentNullException(nameof(random)); if(sectorProvider==null)throw new ArgumentNullException(nameof(sectorProvider)); if(gameplayBoundsQuery==null)throw new ArgumentNullException(nameof(gameplayBoundsQuery)); if(config==null)throw new InvalidOperationException("SpawnConfig is not assigned.");
            var visibility=visibilityQueryBehaviour as ISpawnVisibilityQuery; if(visibility==null)throw new InvalidOperationException("Visibility Query Behaviour must implement ISpawnVisibilityQuery.");
            var navigation=navigationQueryBehaviour as ISpawnNavigationQuery; if(navigation==null)throw new InvalidOperationException("Navigation Query Behaviour must implement ISpawnNavigationQuery.");
            var zombieSpawn=zombieSpawnPortBehaviour as IZombieSpawnPort; if(zombieSpawn==null)throw new InvalidOperationException("Zombie Spawn Port Behaviour must implement IZombieSpawnPort.");
            var population=zombiePopulationQueryBehaviour as IZombiePopulationQuery; if(population==null)throw new InvalidOperationException("Zombie Population Query Behaviour must implement IZombiePopulationQuery.");
            SpawnDifficultyKey initial=config.BuildInitialDifficulty();
            configurator.Initialize(in initial,config.BuildCatalog(),random,sectorProvider,visibility,gameplayBoundsQuery,navigation,zombieSpawn,population,new RandomSpawnSectorSelectionStrategy(),new RandomSpawnPositionStrategy(),config.MaxPlacementAttempts);
            _runtime=runtime; _configurator=configurator; if(config.StartOnInitialize)_runtime.Start();
        }
        public void StartSpawning()=>_runtime?.Start(); public void SetGameplayEnabled(bool enabled)=>_runtime?.SetGameplayEnabled(enabled); public void StopSpawning(SpawnStopReason reason)=>_runtime?.Stop(reason);
        public bool SetDifficulty(int gameLevel,int soldierGroupLevel){if(_runtime==null)return false;var key=new SpawnDifficultyKey(gameLevel,soldierGroupLevel);return _runtime.SetDifficulty(in key);}
        private void Update(){if(_runtime==null||!_runtime.IsInitialized)return;_runtime.Tick(Time.deltaTime);}
        private void OnDestroy(){if(_configurator!=null)_configurator.Shutdown();_runtime=null;_configurator=null;}
    }
}
