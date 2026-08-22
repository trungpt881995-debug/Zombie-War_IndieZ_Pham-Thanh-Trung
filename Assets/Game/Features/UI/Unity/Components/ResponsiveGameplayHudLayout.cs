using UnityEngine;

namespace ZombieWar.Features.UI.Unity.Components
{
    public sealed class ResponsiveGameplayHudLayout : MonoBehaviour
    {
        [SerializeField] private RectTransform weaponBar;
        [SerializeField] private float landscapeWidth = 900f;
        [SerializeField] private float portraitWidth = 680f;
        private bool _landscape;
        private void OnEnable() => Apply(true);
        private void Update() => Apply(false);
        private void Apply(bool force)
        {
            bool land = Screen.width >= Screen.height;
            if (!force && land == _landscape) return;
            _landscape = land;
            if (weaponBar != null) weaponBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, land ? landscapeWidth : portraitWidth);
        }
    }
}
