using System; using System.Collections.Generic; using GeneralCore.Architecture; using NUnit.Framework;
using ZombieWar.Features.Spawn.Catalog; using ZombieWar.Features.Spawn.Domain; using ZombieWar.Features.Spawn.Ports; using ZombieWar.Features.Spawn.Services; using ZombieWar.Features.Spawn.Strategies; using ZombieWar.Features.Spawn.Validation;
namespace ZombieWar.Features.Spawn.Tests
{
    public sealed class SpawnFeatureTests
    {
        private static SpawnTuningCatalog Catalog()=>new SpawnTuningCatalog(new[]{
            Entry(1,1,20,1f,1,2),Entry(1,2,30,.8f,1,2),Entry(1,3,40,.65f,2,3),Entry(1,4,50,.5f,2,4),
            Entry(2,1,25,.8f,1,2),Entry(2,2,35,.65f,2,3),Entry(2,3,45,.5f,2,4),Entry(2,4,60,.4f,3,5)});
        private static SpawnTuningEntry Entry(int gl,int sl,int max,float interval,int min,int maxBatch){var k=new SpawnDifficultyKey(gl,sl);var t=new SpawnTuning(max,interval,min,maxBatch);return new SpawnTuningEntry(in k,in t);}
        private static Ctx Create(int alive=0,bool visible=false,bool inBounds=true,bool nav=true,int randomBatch=1,int attempts=4)
        {
            var c=new Ctx(); c.Events=new EventBus(); c.Runtime=new SpawnRuntime(c.Events); c.Random=new FakeRandom(randomBatch); c.Sectors=new FakeSectors(); c.Visibility=new FakeVisibility(visible); c.Bounds=new FakeBounds(inBounds); c.Nav=new FakeNav(nav); c.Zombies=new FakeZombieSpawner(); c.Population=new FakePopulation{AliveCount=alive};
            var key=new SpawnDifficultyKey(1,1); c.Runtime.Initialize(in key,Catalog(),c.Random,c.Sectors,c.Visibility,c.Bounds,c.Nav,c.Zombies,c.Population,new RandomSpawnSectorSelectionStrategy(),new RandomSpawnPositionStrategy(),attempts); return c;
        }
        [Test] public void Initial_State_Uninitialized(){var r=new SpawnRuntime(new EventBus());Assert.AreEqual(SpawnState.Uninitialized,r.State);}
        [Test] public void Initialize_Moves_To_Ready(){var c=Create();Assert.AreEqual(SpawnState.Ready,c.Runtime.State);}
        [Test] public void Start_Moves_To_Running(){var c=Create();c.Runtime.Start();Assert.AreEqual(SpawnState.Running,c.Runtime.State);}
        [Test] public void Suspend_And_Resume_Work(){var c=Create();c.Runtime.Start();c.Runtime.SetGameplayEnabled(false);Assert.AreEqual(SpawnState.Suspended,c.Runtime.State);c.Runtime.SetGameplayEnabled(true);Assert.AreEqual(SpawnState.Running,c.Runtime.State);}
        [Test] public void Stop_Is_Terminal_Until_Explicit_Start(){var c=Create();c.Runtime.Start();c.Runtime.Stop(SpawnStopReason.BossPhase);c.Runtime.SetGameplayEnabled(true);Assert.AreEqual(SpawnState.Stopped,c.Runtime.State);c.Runtime.Start();Assert.AreEqual(SpawnState.Running,c.Runtime.State);}
        [Test] public void Stop_Stores_Reason(){var c=Create();c.Runtime.Start();c.Runtime.Stop(SpawnStopReason.GameOver);Assert.AreEqual(SpawnState.Stopped,c.Runtime.State);Assert.AreEqual(SpawnStopReason.GameOver,c.Runtime.StopReason);}
        [TestCase(1,1,20,1f,1,2)][TestCase(1,2,30,.8f,1,2)][TestCase(1,3,40,.65f,2,3)][TestCase(1,4,50,.5f,2,4)]
        [TestCase(2,1,25,.8f,1,2)][TestCase(2,2,35,.65f,2,3)][TestCase(2,3,45,.5f,2,4)][TestCase(2,4,60,.4f,3,5)]
        public void Catalog_Returns_Official_Tuning(int gl,int sl,int max,float interval,int min,int maxBatch){var k=new SpawnDifficultyKey(gl,sl);Assert.IsTrue(Catalog().TryGet(in k,out SpawnTuning t));Assert.AreEqual(max,t.MaxAlive);Assert.AreEqual(interval,t.Interval);Assert.AreEqual(min,t.BatchMin);Assert.AreEqual(maxBatch,t.BatchMax);}
        [Test] public void Catalog_Rejects_Duplicate(){var e=Entry(1,1,20,1,1,2);Assert.Throws<ArgumentException>(()=>new SpawnTuningCatalog(new[]{e,e}));}
        [Test] public void Unknown_Difficulty_Returns_False(){var k=new SpawnDifficultyKey(9,1);Assert.IsFalse(Catalog().TryGet(in k,out _));}
        [Test] public void SetDifficulty_Updates_Tuning(){var c=Create();var k=new SpawnDifficultyKey(2,4);Assert.IsTrue(c.Runtime.SetDifficulty(in k));Assert.AreEqual(60,c.Runtime.Tuning.MaxAlive);Assert.AreEqual(.4f,c.Runtime.Tuning.Interval);}
        [Test] public void SetDifficulty_Unknown_Does_Not_Mutate(){var c=Create();SpawnDifficultyKey old=c.Runtime.Difficulty;var k=new SpawnDifficultyKey(9,1);Assert.IsFalse(c.Runtime.SetDifficulty(in k));Assert.AreEqual(old,c.Runtime.Difficulty);}
        [Test] public void NoSpawn_Before_Interval(){var c=Create();c.Runtime.Start();c.Runtime.Tick(.9f);Assert.AreEqual(0,c.Zombies.SpawnCount);}
        [Test] public void Spawn_At_Interval(){var c=Create();c.Runtime.Start();c.Runtime.Tick(1f);Assert.GreaterOrEqual(c.Zombies.SpawnCount,1);}
        [Test] public void Pause_Freezes_Timer(){var c=Create();c.Runtime.Start();c.Runtime.Tick(.7f);float before=c.Runtime.Elapsed;c.Runtime.SetGameplayEnabled(false);c.Runtime.Tick(10);Assert.AreEqual(before,c.Runtime.Elapsed);}
        [Test] public void Resume_No_CatchUp_Burst(){var c=Create();c.Runtime.Start();c.Runtime.Tick(.7f);c.Runtime.SetGameplayEnabled(false);c.Runtime.Tick(10);c.Runtime.SetGameplayEnabled(true);c.Runtime.Tick(.3f);Assert.LessOrEqual(c.Zombies.SpawnCount,2);}
        [Test] public void Large_Delta_Produces_One_Batch(){var c=Create(randomBatch:1);c.Runtime.Start();c.Runtime.Tick(10f);Assert.AreEqual(1,c.Zombies.SpawnCount);}
        [Test] public void Negative_Delta_Is_Sanitized(){var c=Create();c.Runtime.Start();c.Runtime.Tick(-10f);Assert.AreEqual(0f,c.Runtime.Elapsed);}
        [Test] public void At_MaxAlive_Does_Not_Spawn(){var c=Create(alive:20);c.Runtime.Start();c.Runtime.Tick(1f);Assert.AreEqual(0,c.Zombies.SpawnCount);}
        [Test] public void Batch_Is_Clamped_To_Available_Capacity(){var c=Create(alive:19,randomBatch:2);c.Runtime.Start();c.Runtime.Tick(1f);Assert.AreEqual(1,c.Zombies.SpawnCount);}
        [Test] public void Never_Exceeds_MaxAlive(){var c=Create(alive:20,randomBatch:2);c.Runtime.Start();c.Runtime.Tick(2f);Assert.AreEqual(0,c.Zombies.SpawnCount);}
        [TestCase(SpawnSectorId.Top)][TestCase(SpawnSectorId.Bottom)][TestCase(SpawnSectorId.Left)][TestCase(SpawnSectorId.Right)]
        public void SectorProvider_Has_All_Four(SpawnSectorId id){var s=new FakeSectors();Assert.IsTrue(s.TryGetSector(id,out _));}
        [Test] public void Missing_Sector_Is_Safe(){var c=Create();c.Sectors.Enabled=false;c.Runtime.Start();c.Runtime.Tick(1);Assert.AreEqual(0,c.Zombies.SpawnCount);Assert.AreEqual(SpawnRejectReason.NoSector,c.Runtime.LastBatch.LastRejectReason);}
        [Test] public void Visible_Point_Is_Rejected(){var c=Create(visible:true);c.Runtime.Start();c.Runtime.Tick(1);Assert.AreEqual(0,c.Zombies.SpawnCount);Assert.AreEqual(SpawnRejectReason.InsideCamera,c.Runtime.LastBatch.LastRejectReason);}
        [Test] public void Outside_Bounds_Is_Rejected(){var c=Create(inBounds:false);c.Runtime.Start();c.Runtime.Tick(1);Assert.AreEqual(0,c.Zombies.SpawnCount);Assert.AreEqual(SpawnRejectReason.OutsideGameplayBounds,c.Runtime.LastBatch.LastRejectReason);}
        [Test] public void Invalid_Navigation_Is_Rejected(){var c=Create(nav:false);c.Runtime.Start();c.Runtime.Tick(1);Assert.AreEqual(0,c.Zombies.SpawnCount);Assert.AreEqual(SpawnRejectReason.InvalidNavigation,c.Runtime.LastBatch.LastRejectReason);}
        [Test] public void Navigation_Resolved_Point_Is_Used(){var c=Create();c.Nav.OffsetY=3;c.Runtime.Start();c.Runtime.Tick(1);Assert.AreEqual(3f,c.Zombies.Last.Y);}
        [Test] public void Placement_Retries_After_Invalid(){var c=Create(attempts:3);c.Visibility.VisibleCallsRemaining=1;c.Runtime.Start();c.Runtime.Tick(1);Assert.AreEqual(1,c.Zombies.SpawnCount);Assert.GreaterOrEqual(c.Visibility.Calls,2);}
        [Test] public void Placement_Stops_After_MaxAttempts(){var c=Create(visible:true,attempts:3);c.Runtime.Start();c.Runtime.Tick(1);Assert.AreEqual(3,c.Visibility.Calls);Assert.AreEqual(0,c.Zombies.SpawnCount);}
        [Test] public void Pool_Failure_Is_Safe(){var c=Create(randomBatch:2);c.Zombies.Allow=false;c.Runtime.Start();c.Runtime.Tick(1);Assert.AreEqual(0,c.Zombies.SpawnCount);Assert.AreEqual(SpawnRejectReason.PoolUnavailable,c.Runtime.LastBatch.LastRejectReason);}
        [Test] public void Pool_Failure_Stops_Current_Batch(){var c=Create(randomBatch:2);c.Zombies.FailAfter=1;c.Runtime.Start();c.Runtime.Tick(1);Assert.AreEqual(1,c.Zombies.SpawnCount);Assert.AreEqual(2,c.Zombies.TryCount);}
        [Test] public void Valid_Point_Calls_Spawn_Port(){var c=Create();c.Runtime.Start();c.Runtime.Tick(1);Assert.Greater(c.Zombies.TryCount,0);}
        [Test] public void Stop_BossPhase_Does_Not_Touch_Existing_Population(){var c=Create(alive:7);c.Runtime.Start();c.Runtime.Stop(SpawnStopReason.BossPhase);Assert.AreEqual(7,c.Population.AliveCount);Assert.AreEqual(0,c.Zombies.SpawnCount);}
        [Test] public void Stopped_Runtime_Does_Not_Spawn(){var c=Create();c.Runtime.Start();c.Runtime.Stop(SpawnStopReason.LevelComplete);c.Runtime.Tick(10);Assert.AreEqual(0,c.Zombies.SpawnCount);}
        [Test] public void Suspended_Runtime_Does_Not_Spawn(){var c=Create();c.Runtime.Start();c.Runtime.SetGameplayEnabled(false);c.Runtime.Tick(10);Assert.AreEqual(0,c.Zombies.SpawnCount);}
        [Test] public void Shutdown_Resets_Runtime(){var c=Create();((ISpawnRuntimeConfigurator)c.Runtime).Shutdown();Assert.AreEqual(SpawnState.Uninitialized,c.Runtime.State);Assert.IsFalse(c.Runtime.IsInitialized);}
        [Test] public void Point_Rejects_Nan()=>Assert.Throws<ArgumentOutOfRangeException>(()=>new SpawnPoint(float.NaN,0,0));
        [Test] public void Tuning_Rejects_NonPositive_Max()=>Assert.Throws<ArgumentOutOfRangeException>(()=>new SpawnTuning(0,1,1,1));
        [Test] public void Tuning_Rejects_NonPositive_Interval()=>Assert.Throws<ArgumentOutOfRangeException>(()=>new SpawnTuning(10,0,1,1));
        [Test] public void Tuning_Rejects_Invalid_Batch()=>Assert.Throws<ArgumentOutOfRangeException>(()=>new SpawnTuning(10,1,3,2));
        [Test] public void Difficulty_Rejects_Invalid_Level()=>Assert.Throws<ArgumentOutOfRangeException>(()=>new SpawnDifficultyKey(0,1));
        [Test] public void Random_Position_Stays_In_Area(){var a=new SpawnArea(-10,10,-5,5);var r=new FakeRandom(1){Values=new Queue<float>(new[]{.25f,.75f})};SpawnPoint p=new RandomSpawnPositionStrategy().Select(in a,r);Assert.IsTrue(a.Contains(in p));}
        [Test] public void Validator_Order_Stops_At_Visibility(){var v=new FakeVisibility(true);var b=new FakeBounds(true);var n=new FakeNav(true);var val=new SpawnPlacementValidator(v,b,n);var p=new SpawnPoint(0,0,0);val.Validate(in p);Assert.AreEqual(0,b.Calls);Assert.AreEqual(0,n.Calls);}
        [Test] public void Validator_Order_Stops_At_Bounds(){var v=new FakeVisibility(false);var b=new FakeBounds(false);var n=new FakeNav(true);var val=new SpawnPlacementValidator(v,b,n);var p=new SpawnPoint(0,0,0);val.Validate(in p);Assert.AreEqual(1,b.Calls);Assert.AreEqual(0,n.Calls);}
        [Test] public void Events_Start_And_Stop_Are_Low_Frequency(){var c=Create();int starts=0,stops=0;c.Events.Subscribe<ZombieWar.Features.Spawn.Events.SpawnStartedEvent>(_=>starts++);c.Events.Subscribe<ZombieWar.Features.Spawn.Events.SpawnStoppedEvent>(_=>stops++);c.Runtime.Start();c.Runtime.Tick(5);c.Runtime.Stop(SpawnStopReason.Manual);Assert.AreEqual(1,starts);Assert.AreEqual(1,stops);}
        [Test] public void Same_Difficulty_Does_Not_Reset_Elapsed(){var c=Create();c.Runtime.Start();c.Runtime.Tick(.5f);var k=new SpawnDifficultyKey(1,1);Assert.IsTrue(c.Runtime.SetDifficulty(in k));Assert.AreEqual(.5f,c.Runtime.Elapsed);}
        [Test] public void Changed_Difficulty_Resets_Timer(){var c=Create();c.Runtime.Start();c.Runtime.Tick(.5f);var k=new SpawnDifficultyKey(1,2);c.Runtime.SetDifficulty(in k);Assert.AreEqual(0f,c.Runtime.Elapsed);}
        [Test] public void MaxAlive_Full_Consumes_Interval_No_Backlog(){var c=Create(alive:20);c.Runtime.Start();c.Runtime.Tick(5);Assert.AreEqual(0f,c.Runtime.Elapsed);c.Population.AliveCount=19;c.Runtime.Tick(.99f);Assert.AreEqual(0,c.Zombies.SpawnCount);c.Runtime.Tick(.02f);Assert.AreEqual(1,c.Zombies.SpawnCount);}

        private sealed class Ctx { public EventBus Events; public SpawnRuntime Runtime; public FakeRandom Random; public FakeSectors Sectors; public FakeVisibility Visibility; public FakeBounds Bounds; public FakeNav Nav; public FakeZombieSpawner Zombies; public FakePopulation Population; }
        private sealed class FakeRandom:ISpawnRandom { private readonly int _batch; public Queue<float> Values; public FakeRandom(int batch){_batch=batch;} public int Range(int min,int max){if(min==0&&max==4)return 0;return Math.Max(min,Math.Min(max-1,_batch));} public float Value()=>Values!=null&&Values.Count>0?Values.Dequeue():.5f; }
        private sealed class FakeSectors:ISpawnSectorProvider { public bool Enabled=true; public bool TryGetSector(SpawnSectorId id,out SpawnSector sector){sector=default;if(!Enabled)return false;var a=new SpawnArea(-10,10,-10,10);sector=new SpawnSector(id,in a);return true;} }
        private sealed class FakeVisibility:ISpawnVisibilityQuery { private readonly bool _default; public int VisibleCallsRemaining; public int Calls; public FakeVisibility(bool value){_default=value;} public bool IsVisible(in SpawnPoint p){Calls++;if(VisibleCallsRemaining>0){VisibleCallsRemaining--;return true;}return _default;} }
        private sealed class FakeBounds:ISpawnGameplayBoundsQuery { private readonly bool _value; public int Calls; public FakeBounds(bool value){_value=value;} public bool Contains(in SpawnPoint p){Calls++;return _value;} }
        private sealed class FakeNav:ISpawnNavigationQuery { private readonly bool _value; public int Calls; public float OffsetY; public FakeNav(bool value){_value=value;} public bool TryResolve(in SpawnPoint c,out SpawnPoint r){Calls++;if(!_value){r=default;return false;}r=new SpawnPoint(c.X,c.Y+OffsetY,c.Z);return true;} }
        private sealed class FakeZombieSpawner:IZombieSpawnPort { public bool Allow=true; public int FailAfter=int.MaxValue; public int TryCount; public int SpawnCount; public SpawnPoint Last; public bool TrySpawn(in SpawnPoint p){TryCount++;if(!Allow||SpawnCount>=FailAfter)return false;SpawnCount++;Last=p;return true;} }
        private sealed class FakePopulation:IZombiePopulationQuery { public int AliveCount { get; set; } }
    }
}
