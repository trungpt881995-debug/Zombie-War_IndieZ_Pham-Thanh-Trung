using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Spawn.Catalog;
using ZombieWar.Features.Spawn.Domain;
using ZombieWar.Features.Spawn.Events;
using ZombieWar.Features.Spawn.Model;
using ZombieWar.Features.Spawn.Ports;
using ZombieWar.Features.Spawn.Strategies;
using ZombieWar.Features.Spawn.Validation;
namespace ZombieWar.Features.Spawn.Controller
{
    public sealed class SpawnController
    {
        private readonly SpawnModel _model; private readonly ISpawnTuningCatalog _catalog; private readonly ISpawnRandom _random;
        private readonly ISpawnSectorProvider _sectors; private readonly ISpawnSectorSelectionStrategy _sectorSelector; private readonly ISpawnPositionStrategy _positionSelector;
        private readonly ISpawnPlacementValidator _validator; private readonly IZombieSpawnPort _zombies; private readonly IZombiePopulationQuery _population; private readonly IEventBus _events;
        private readonly int _maxPlacementAttempts;
        public SpawnController(SpawnModel model,ISpawnTuningCatalog catalog,ISpawnRandom random,ISpawnSectorProvider sectors,ISpawnSectorSelectionStrategy sectorSelector,ISpawnPositionStrategy positionSelector,ISpawnPlacementValidator validator,IZombieSpawnPort zombies,IZombiePopulationQuery population,IEventBus events,int maxPlacementAttempts)
        {
            _model=model??throw new ArgumentNullException(nameof(model)); _catalog=catalog??throw new ArgumentNullException(nameof(catalog)); _random=random??throw new ArgumentNullException(nameof(random));
            _sectors=sectors??throw new ArgumentNullException(nameof(sectors)); _sectorSelector=sectorSelector??throw new ArgumentNullException(nameof(sectorSelector)); _positionSelector=positionSelector??throw new ArgumentNullException(nameof(positionSelector));
            _validator=validator??throw new ArgumentNullException(nameof(validator)); _zombies=zombies??throw new ArgumentNullException(nameof(zombies)); _population=population??throw new ArgumentNullException(nameof(population)); _events=events??throw new ArgumentNullException(nameof(events));
            if(maxPlacementAttempts<=0) throw new ArgumentOutOfRangeException(nameof(maxPlacementAttempts)); _maxPlacementAttempts=maxPlacementAttempts;
        }
        public void Initialize(in SpawnDifficultyKey key)
        {
            if(_model.State!=SpawnState.Uninitialized) throw new InvalidOperationException("SpawnController is already initialized.");
            if(!_catalog.TryGet(in key,out SpawnTuning tuning)) throw new InvalidOperationException($"Spawn tuning not found for {key}.");
            _model.Initialize(in key,in tuning); _events.Publish(new SpawnTuningChangedEvent(in key,in tuning));
        }
        public bool SetDifficulty(in SpawnDifficultyKey key)
        {
            if(_model.State==SpawnState.Uninitialized) return false;
            if(!_catalog.TryGet(in key,out SpawnTuning tuning)) return false;
            if(_model.Difficulty.Equals(key) && _model.Tuning.Equals(tuning)) return true;
            _model.SetDifficulty(in key,in tuning); _events.Publish(new SpawnTuningChangedEvent(in key,in tuning)); return true;
        }
        public void Start()
        {
            if(_model.State!=SpawnState.Ready && _model.State!=SpawnState.Stopped) return;
            _model.Start(); SpawnDifficultyKey key=_model.Difficulty; _events.Publish(new SpawnStartedEvent(in key));
        }
        public void SetGameplayEnabled(bool enabled)
        {
            if(_model.State==SpawnState.Uninitialized || _model.State==SpawnState.Stopped) return;
            if(enabled) _model.Resume(); else _model.Suspend();
        }
        public void Stop(SpawnStopReason reason)
        {
            if(_model.State==SpawnState.Uninitialized || _model.State==SpawnState.Stopped) return;
            _model.Stop(reason); _events.Publish(new SpawnStoppedEvent(reason));
        }
        public void Tick(float deltaTime)
        {
            if(_model.State!=SpawnState.Running) return;
            _model.Advance(SanitizeDelta(deltaTime));
            if(!_model.IntervalReady) return;
            _model.ConsumeInterval(); // deliberately no multi-batch catch-up in one frame
            TrySpawnBatch();
        }
        private void TrySpawnBatch()
        {
            int alive=Math.Max(0,_population.AliveCount); int available=_model.Tuning.MaxAlive-alive;
            if(available<=0) { SpawnBatchResult full=new SpawnBatchResult(0,0,0,SpawnRejectReason.None); _model.RecordBatch(in full); return; }
            int desired=_random.Range(_model.Tuning.BatchMin,_model.Tuning.BatchMax+1); int count=Math.Min(desired,available); int spawned=0; SpawnRejectReason last=SpawnRejectReason.None;
            for(int i=0;i<count;i++)
            {
                if(!TryFindSpawnPoint(out SpawnPoint point,out last)) continue;
                if(!_zombies.TrySpawn(in point)) { last=SpawnRejectReason.PoolUnavailable; break; }
                spawned++;
            }
            SpawnBatchResult result=new SpawnBatchResult(desired,count,spawned,last); _model.RecordBatch(in result);
        }
        private bool TryFindSpawnPoint(out SpawnPoint point,out SpawnRejectReason reason)
        {
            point=default; reason=SpawnRejectReason.AttemptsExhausted;
            for(int attempt=0;attempt<_maxPlacementAttempts;attempt++)
            {
                SpawnSectorId id=_sectorSelector.Select(_random);
                if(!_sectors.TryGetSector(id,out SpawnSector sector)) { reason=SpawnRejectReason.NoSector; continue; }
                SpawnArea area=sector.Area; SpawnPoint candidate=_positionSelector.Select(in area,_random);
                SpawnPlacementResult validation=_validator.Validate(in candidate);
                if(!validation.IsValid) { reason=validation.RejectReason; continue; }
                point=validation.ResolvedPoint; reason=SpawnRejectReason.None; return true;
            }
            return false;
        }
        public void Shutdown() => _model.Reset();
        private static float SanitizeDelta(float dt) => float.IsNaN(dt)||float.IsInfinity(dt)||dt<0f ? 0f : dt;
    }
}
