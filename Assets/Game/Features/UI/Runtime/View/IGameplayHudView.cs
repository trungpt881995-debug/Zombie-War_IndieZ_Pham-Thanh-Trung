using System;
using ZombieWar.Features.UI.Domain;

namespace ZombieWar.Features.UI.View
{
    public interface IGameplayHudView : IUIScreenView
    {
        event Action PauseClicked;
        event Action < UIWeaponId > WeaponClicked;
        void SetScore(long score);
        void SetGameLevel(int level);
        void SetSoldierGroupLevel(int level);
        void SetHealth(float normalized, float current, float max);
        void SetWeaponState(UIWeaponId weapon, bool selected, float cooldownNormalized, bool interactable);
    }
}
