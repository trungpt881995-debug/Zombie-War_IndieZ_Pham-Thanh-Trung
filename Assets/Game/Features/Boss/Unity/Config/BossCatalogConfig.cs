using System; using UnityEngine; using ZombieWar.Features.Boss.Catalog; using ZombieWar.Features.Boss.Domain;
namespace ZombieWar.Features.Boss.Unity.Config
{
    [CreateAssetMenu(fileName="BossCatalogConfig",menuName="Zombie War/Boss/Boss Catalog Config")]
    public sealed class BossCatalogConfig:ScriptableObject
    {
        [SerializeField] private BossConfig[] bosses=new BossConfig[3];
        public BossCatalog CreateCatalog(){if(bosses==null||bosses.Length!=3)throw new InvalidOperationException("BossCatalogConfig requires exactly three BossConfig assets.");var defs=new BossDefinition[3];for(int i=0;i<3;i++){if(bosses[i]==null)throw new InvalidOperationException($"BossConfig at index {i} is missing.");defs[i]=bosses[i].CreateDefinition();}return new BossCatalog(defs);}
        public bool TryGetConfig(BossId id,out BossConfig config){if(bosses!=null)for(int i=0;i<bosses.Length;i++)if(bosses[i]!=null&&bosses[i].BossId==id){config=bosses[i];return true;}config=null;return false;}
    }
}
