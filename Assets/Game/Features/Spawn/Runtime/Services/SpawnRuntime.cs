using System; using GeneralCore.Architecture; using ZombieWar.Features.Spawn.Catalog; using ZombieWar.Features.Spawn.Controller; using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Model; using ZombieWar.Features.Spawn.Ports; using ZombieWar.Features.Spawn.Strategies; using ZombieWar.Features.Spawn.Validation;
namespace ZombieWar.Features.Spawn.Services
{
    public sealed class SpawnRuntime : ISpawnRuntime, ISpawnRuntimeConfigurator
    {
        private readonly IEventBus _events; 
        private SpawnModel _model; 
        private SpawnController _controller;
        public bool IsInitialized=>_controller!=null; 
        public SpawnState State=>IsInitialized?_model.State:SpawnState.Uninitialized; 
        public bool GameplayEnabled=>State==SpawnState.Running;
        public SpawnDifficultyKey Difficulty=>IsInitialized?_model.Difficulty:default; 
        public SpawnTuning Tuning=>IsInitialized?_model.Tuning:default; 
        public float Elapsed=>IsInitialized?_model.Elapsed:0f; 
        public SpawnStopReason StopReason=>IsInitialized?_model.StopReason:SpawnStopReason.None;
        public SpawnBatchResult LastBatch=>IsInitialized?_model.LastBatch:default; 
        public int SuccessfulSpawnCount=>IsInitialized?_model.SuccessfulSpawnCount:0; 
        public int RejectedSpawnCount=>IsInitialized?_model.RejectedSpawnCount:0;
        public SpawnRuntime(IEventBus events) 
        { 
            _events=events??throw new ArgumentNullException(nameof(events)); 
        }
        public void Initialize(in SpawnDifficultyKey initialDifficulty,ISpawnTuningCatalog catalog,ISpawnRandom random,ISpawnSectorProvider sectorProvider,ISpawnVisibilityQuery visibilityQuery,ISpawnGameplayBoundsQuery gameplayBoundsQuery,ISpawnNavigationQuery navigationQuery,IZombieSpawnPort zombieSpawnPort,IZombiePopulationQuery populationQuery,ISpawnSectorSelectionStrategy sectorSelectionStrategy,ISpawnPositionStrategy positionStrategy,int maxPlacementAttempts)
        {
            if(IsInitialized) 
            throw new InvalidOperationException("SpawnRuntime is already initialized.");

            _model=new SpawnModel(); var validator=new SpawnPlacementValidator(visibilityQuery,gameplayBoundsQuery,navigationQuery);
            _controller=new SpawnController(_model,catalog,random,sectorProvider,sectorSelectionStrategy,positionStrategy,validator,zombieSpawnPort,populationQuery,_events,maxPlacementAttempts);
            _controller.Initialize(in initialDifficulty);
        }
        public void Tick(float deltaTime)
        { 
            if(IsInitialized) 
            _controller.Tick(deltaTime); 
        }
        public void Start()
        { 
            if(IsInitialized) 
            _controller.Start(); 
        }
        public void SetGameplayEnabled(bool enabled)
        { 
            if(IsInitialized) 
            _controller.SetGameplayEnabled(enabled); 
        }
        public void Stop(SpawnStopReason reason)
        { 
            if(IsInitialized) 
            _controller.Stop(reason); 
        }
        public bool SetDifficulty(in SpawnDifficultyKey key)=>IsInitialized&&_controller.SetDifficulty(in key);
        public void Shutdown()
        { 
            if(!IsInitialized)return; 
            _controller.Shutdown(); 
            _controller=null; 
            _model=null; 
        }
    }
}
