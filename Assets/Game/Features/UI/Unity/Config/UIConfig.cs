using TMPro;
using UnityEngine;

namespace ZombieWar.Features.UI.Unity.Config
{
    [CreateAssetMenu(menuName = "Zombie War/UI/UI Config", fileName = "UIConfig")] public sealed class UIConfig : ScriptableObject
    {
        public string gameTitle = "ZOMBIE WAR";
        public bool showEndGameReplay = true;
        public Sprite logo;
        public Sprite mainMenuLandscape;
        public Sprite mainMenuPortrait;
        public Sprite commonPanel;
        public Sprite commonButton;
        public Sprite pauseIcon;
        public TMP_FontAsset font;
    }
}
