using UnityEngine;
using UnityEngine.UI;
using ZombieWar.Features.Feedback.Domain;

namespace ZombieWar.Features.Feedback.Unity.View
{
    public sealed class ScreenFlashView : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Color damageColor = new Color(1f, 0.05f, 0.05f, 1f);
        [SerializeField] private Color impactColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color positiveColor = new Color(0.2f, 1f, 0.4f, 1f);

        private bool _suspended;
        private float _duration;
        private float _elapsed;
        private float _peakAlpha;

        public bool IsActive => image != null && image.enabled && _peakAlpha > 0f;

        private void Awake()
        {
            if (image == null)
            {
                image = GetComponent<Image>();
            }

            if (image != null)
            {
                image.raycastTarget = false;
                image.enabled = false;
            }
        }

        private void Update()
        {
            if (_suspended ||
                image == null ||
                !image.enabled ||
                _duration <= 0f)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;

            float normalized = Mathf.Clamp01(_elapsed / _duration);
            float alpha = _peakAlpha * (1f - normalized);

            Color color = image.color;
            color.a = alpha;
            image.color = color;

            if (normalized >= 1f)
            {
                Clear();
            }
        }

        public void Flash(
            ScreenFeedbackKind kind,
            float intensity,
            float duration)
        {
            if (image == null || duration <= 0f || intensity <= 0f)
            {
                return;
            }

            Color color = ResolveColor(kind);
            color.a = Mathf.Clamp01(intensity);

            image.color = color;
            image.enabled = true;

            _duration = duration;
            _elapsed = 0f;
            _peakAlpha = color.a;
        }

        public void SetSuspended(bool suspended)
        {
            _suspended = suspended;
        }

        public void Clear()
        {
            _duration = 0f;
            _elapsed = 0f;
            _peakAlpha = 0f;

            if (image == null)
            {
                return;
            }

            Color color = image.color;
            color.a = 0f;
            image.color = color;
            image.enabled = false;
        }

        private Color ResolveColor(ScreenFeedbackKind kind)
        {
            switch (kind)
            {
                case ScreenFeedbackKind.Damage:
                    return damageColor;

                case ScreenFeedbackKind.Positive:
                    return positiveColor;

                case ScreenFeedbackKind.Impact:
                default:
                    return impactColor;
            }
        }
    }
}
