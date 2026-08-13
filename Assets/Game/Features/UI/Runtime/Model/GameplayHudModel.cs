using ZombieWar.Features.UI.Domain;
namespace ZombieWar.Features.UI.Model
{
    public sealed class GameplayHudModel
    {
        private readonly float[] _cooldowns = new float[6];
        private readonly bool[] _interactable = new bool[6];
        public long Score { get; internal set; }
        public int GameLevel { get; internal set; }
        public int SoldierGroupLevel { get; internal set; }
        public float HealthNormalized { get; internal set; } = 1f;
        public float CurrentHealth { get; internal set; }
        public float MaxHealth { get; internal set; }
        public UIWeaponId SelectedWeapon { get; internal set; } = UIWeaponId.Pistol;
        public float GetCooldown(UIWeaponId id) => _cooldowns[(int)id];
        public bool GetInteractable(UIWeaponId id) => _interactable[(int)id];
        internal void SetWeapon(UIWeaponId id,float cooldown,bool interactable){_cooldowns[(int)id]=Clamp01(cooldown);_interactable[(int)id]=interactable;}
        private static float Clamp01(float v)=>v<0f?0f:(v>1f?1f:v);
    }
}
