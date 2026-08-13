using System; using UnityEngine; using ZombieWar.Features.Spawn.Catalog; using ZombieWar.Features.Spawn.Domain;
namespace ZombieWar.Features.Spawn.Unity.Config
{
    [CreateAssetMenu(menuName="Zombie War/Spawn/Spawn Config",fileName="SpawnConfig")]
    public sealed class SpawnConfig : ScriptableObject
    {
        [Serializable] private struct Entry
        {
            [Min(1)] public int gameLevel; [Range(1,4)] public int soldierGroupLevel; [Min(1)] public int maxAlive; [Min(0.01f)] public float interval; [Min(1)] public int batchMin; [Min(1)] public int batchMax;
            public SpawnTuningEntry Build(){var key=new SpawnDifficultyKey(gameLevel,soldierGroupLevel);var t=new SpawnTuning(maxAlive,interval,batchMin,batchMax);return new SpawnTuningEntry(in key,in t);}
        }
        [SerializeField] private bool startOnInitialize=true;
        [SerializeField,Min(1)] private int initialGameLevel=1;
        [SerializeField,Range(1,4)] private int initialSoldierGroupLevel=1;
        [SerializeField,Min(1)] private int maxPlacementAttempts=8;
        [SerializeField] private Entry[] profiles=CreateDefaults();
        public bool StartOnInitialize=>startOnInitialize; public int MaxPlacementAttempts=>Mathf.Max(1,maxPlacementAttempts);
        public SpawnDifficultyKey BuildInitialDifficulty()=>new SpawnDifficultyKey(Mathf.Max(1,initialGameLevel),Mathf.Clamp(initialSoldierGroupLevel,1,4));
        public SpawnTuningCatalog BuildCatalog()
        {
            if(profiles==null||profiles.Length==0) throw new InvalidOperationException("SpawnConfig requires tuning profiles.");
            var entries=new SpawnTuningEntry[profiles.Length]; for(int i=0;i<profiles.Length;i++) entries[i]=profiles[i].Build(); return new SpawnTuningCatalog(entries);
        }
        private void Reset(){profiles=CreateDefaults();}
        private static Entry[] CreateDefaults()=>new[]
        {
            E(1,1,20,1.0f,1,2), E(1,2,30,0.8f,1,2), E(1,3,40,0.65f,2,3), E(1,4,50,0.5f,2,4),
            E(2,1,25,0.8f,1,2), E(2,2,35,0.65f,2,3), E(2,3,45,0.5f,2,4), E(2,4,60,0.4f,3,5)
        };
        private static Entry E(int gl,int sl,int max,float interval,int min,int maxBatch)=>new Entry{gameLevel=gl,soldierGroupLevel=sl,maxAlive=max,interval=interval,batchMin=min,batchMax=maxBatch};
    }
}
