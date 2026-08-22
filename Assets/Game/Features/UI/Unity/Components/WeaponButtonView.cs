using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZombieWar.Features.UI.Domain;

namespace ZombieWar.Features.UI.Unity.Components
{
    public sealed class WeaponButtonView : MonoBehaviour
    {
        [SerializeField] private UIWeaponId weapon;
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private GameObject selectedIndicator;
        public UIWeaponId Weapon => weapon;
        public event Action < UIWeaponId > Clicked;
        private void Awake()
        {
            if (icon != null) icon.raycastTarget = false;
            if (cooldownFill != null) cooldownFill.raycastTarget = false;
            if (button != null) button.onClick.AddListener(OnClick);
        }
        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(OnClick);
        }
        private void OnClick() => Clicked?.Invoke(weapon);
        public void Render(bool selected, float cooldown, bool interactable)
        {
            if (selectedIndicator != null) selectedIndicator.SetActive(selected);
            if (cooldownFill != null) cooldownFill.fillAmount = Mathf.Clamp01(cooldown);
            if (button != null) button.interactable = interactable;
        }
        public void Apply(string displayName, Sprite sprite)
        {
            if (label != null) label.text = displayName;
            if (icon != null)
            {
                icon.sprite = sprite;
                icon.enabled = sprite != null;
            }
        }
    }
}
