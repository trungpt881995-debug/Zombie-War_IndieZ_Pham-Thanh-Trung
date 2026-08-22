using UnityEngine;
using ZombieWar.Features.UI.Domain;
using ZombieWar.Features.UI.View;

namespace ZombieWar.Features.UI.Unity.Components
{
    public abstract class CanvasGroupScreenView : MonoBehaviour, IUIScreenView
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        public abstract UIScreenId ScreenId
        {
            get;
        }
        public virtual void SetVisible(bool visible)
        {
            if (canvasGroup == null) canvasGroup = GetComponent < CanvasGroup > ();
            if (canvasGroup == null) return;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}
