using System.Collections.Generic; using UnityEditor; using UnityEngine; using ZombieWar.Features.VFX.Domain; using ZombieWar.Features.VFX.Unity.Config; using ZombieWar.Features.VFX.Unity.View;
namespace ZombieWar.Features.VFX.Editor
{
    public static class ZombieWarVFXPlaceholderBuilder
    {
        private const string Root="Assets/GameGenerated/VFX";
        [MenuItem("Tools/Zombie War/VFX/Create Placeholder VFX Library")]
        public static void Create()
        {
            Ensure(Root);Ensure(Root+"/Prefabs");Ensure(Root+"/Config");
            var configs=new List<VFXConfig>();
            Add(configs,VFXId.PistolMuzzle,.12f,8,32,false);Add(configs,VFXId.AKMuzzle,.10f,12,48,false);Add(configs,VFXId.ShotgunMuzzle,.18f,6,24,false);Add(configs,VFXId.SniperMuzzle,.16f,4,16,false);Add(configs,VFXId.GrenadeMuzzle,.22f,4,16,false);
            Add(configs,VFXId.FlamethrowerLoop,1f,4,8,false,VFXLifetimeMode.Looping);
            Add(configs,VFXId.BulletImpact,.25f,24,64,true);Add(configs,VFXId.BloodImpact,.35f,24,64,true);Add(configs,VFXId.SoldierDamage,.35f,8,24,true);Add(configs,VFXId.ZombieHit,.25f,16,48,true);Add(configs,VFXId.ZombieDeath,.8f,12,32,true);
            Add(configs,VFXId.GrenadeExplosion,1.25f,8,24,true);Add(configs,VFXId.BossSpawn,1f,3,6,true);Add(configs,VFXId.BossHit,.45f,8,24,true);Add(configs,VFXId.BossDeath,2f,3,6,true);
            string catalogPath=Root+"/Config/VFXCatalog_Game.asset";var catalog=AssetDatabase.LoadAssetAtPath<VFXCatalogConfig>(catalogPath);if(catalog==null){catalog=ScriptableObject.CreateInstance<VFXCatalogConfig>();AssetDatabase.CreateAsset(catalog,catalogPath);}catalog.effects=configs.ToArray();EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();AssetDatabase.Refresh();Selection.activeObject=catalog;Debug.Log("[ZombieWar VFX] Placeholder library created at "+Root);
        }
        private static void Add(List<VFXConfig> list,VFXId id,float duration,int prewarm,int max,bool terminal,VFXLifetimeMode lifetime=VFXLifetimeMode.OneShot)
        {
            string prefabPath=Root+"/Prefabs/VFX_"+id+".prefab";ParticleVFXView prefab=AssetDatabase.LoadAssetAtPath<ParticleVFXView>(prefabPath);if(prefab==null){var go=new GameObject("VFX_"+id);var ps=go.AddComponent<ParticleSystem>();var main=ps.main;main.loop=lifetime==VFXLifetimeMode.Looping;main.duration=Mathf.Max(.1f,duration);main.startLifetime=Mathf.Max(.08f,duration*.55f);main.startSpeed=id==VFXId.GrenadeExplosion||id==VFXId.BossDeath?5f:2f;main.startSize=id==VFXId.GrenadeExplosion||id==VFXId.BossDeath?1.2f:.28f;var emission=ps.emission;if(lifetime==VFXLifetimeMode.Looping){emission.rateOverTime=36f;}else{emission.rateOverTime=0f;emission.SetBursts(new[]{new ParticleSystem.Burst(0f,(short)(id==VFXId.GrenadeExplosion||id==VFXId.BossDeath?32:10))});}var shape=ps.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=id==VFXId.ShotgunMuzzle?32f:15f;prefab=go.AddComponent<ParticleVFXView>();PrefabUtility.SaveAsPrefabAsset(go,prefabPath);Object.DestroyImmediate(go);prefab=AssetDatabase.LoadAssetAtPath<ParticleVFXView>(prefabPath);}
            string cfgPath=Root+"/Config/VFX_"+id+".asset";var cfg=AssetDatabase.LoadAssetAtPath<VFXConfig>(cfgPath);if(cfg==null){cfg=ScriptableObject.CreateInstance<VFXConfig>();AssetDatabase.CreateAsset(cfg,cfgPath);}cfg.id=id;cfg.prefab=prefab;cfg.lifetime=lifetime;cfg.duration=duration;cfg.allowDuringTerminalDrain=terminal;cfg.prewarmCount=prewarm;cfg.maxCapacity=max;cfg.allowGrowth=true;cfg.defaultScale=1f;EditorUtility.SetDirty(cfg);list.Add(cfg);
        }
        private static void Ensure(string path){if(AssetDatabase.IsValidFolder(path))return;int slash=path.LastIndexOf('/');string parent=path.Substring(0,slash),name=path.Substring(slash+1);Ensure(parent);AssetDatabase.CreateFolder(parent,name);}
    }
}
