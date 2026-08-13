using System; using UnityEngine; using ZombieWar.Features.Level.Catalog; using ZombieWar.Features.Level.Domain;
namespace ZombieWar.Features.Level.Unity.Config
{
    [CreateAssetMenu(menuName="Zombie War/Level/Level Catalog Config",fileName="LevelCatalogConfig")]
    public sealed class LevelCatalogConfig:ScriptableObject
    {
        [SerializeField] private LevelConfig[] levels;
        public LevelCatalog BuildCatalog(){if(levels==null||levels.Length==0)throw new InvalidOperationException("LevelCatalogConfig requires LevelConfig assets.");var defs=new LevelDefinition[levels.Length];for(int i=0;i<levels.Length;i++){if(levels[i]==null)throw new InvalidOperationException($"Level config at index {i} is null.");defs[i]=levels[i].BuildDefinition();}return new LevelCatalog(defs);}
    }
}
