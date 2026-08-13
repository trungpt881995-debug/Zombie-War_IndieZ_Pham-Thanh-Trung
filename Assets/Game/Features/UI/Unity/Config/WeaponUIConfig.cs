using System; using UnityEngine; using ZombieWar.Features.UI.Domain;
namespace ZombieWar.Features.UI.Unity.Config
{
    [Serializable] public struct WeaponUIEntry { public UIWeaponId weapon; public string displayName; public Sprite icon; }
    [CreateAssetMenu(menuName="Zombie War/UI/Weapon UI Config",fileName="WeaponUIConfig")]
    public sealed class WeaponUIConfig:ScriptableObject
    {
        public WeaponUIEntry[] entries=new[]{
            new WeaponUIEntry{weapon=UIWeaponId.Pistol,displayName="Pistol"},new WeaponUIEntry{weapon=UIWeaponId.AK,displayName="AK"},new WeaponUIEntry{weapon=UIWeaponId.Shotgun,displayName="Shotgun"},new WeaponUIEntry{weapon=UIWeaponId.SniperRifle,displayName="Sniper"},new WeaponUIEntry{weapon=UIWeaponId.GrenadeLauncher,displayName="Grenade"},new WeaponUIEntry{weapon=UIWeaponId.Flamethrower,displayName="Flame"}};
        public bool TryGet(UIWeaponId id,out WeaponUIEntry entry){if(entries!=null)for(int i=0;i<entries.Length;i++)if(entries[i].weapon==id){entry=entries[i];return true;}entry=default;return false;}
    }
}
