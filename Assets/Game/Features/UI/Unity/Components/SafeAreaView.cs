using UnityEngine;

namespace ZombieWar.Features.UI.Unity.Components
{
    [RequireComponent(typeof(RectTransform))] public sealed class SafeAreaView : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _last;
        private int _w = - 1, _h = - 1;
        private void Awake()
        {
            _rect = (RectTransform) transform;
            Apply();
        }
        private void Update()
        {
            if (_last != Screen.safeArea || _w != Screen.width || _h != Screen.height) Apply();
        }
        private void Apply()
        {
            _last = Screen.safeArea;
            _w = Screen.width;
            _h = Screen.height;
            if (_w <= 0 || _h <= 0) return;
            Vector2 min = _last.position, max = _last.position + _last.size;
            min.x /= _w;
            min.y /= _h;
            max.x /= _w;
            max.y /= _h;
            _rect.anchorMin = min;
            _rect.anchorMax = max;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
