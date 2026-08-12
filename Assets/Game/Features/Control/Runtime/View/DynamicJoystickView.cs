using System;
using UnityEngine;
using UnityEngine.EventSystems;
using ZombieWar.Features.Control.Domain;

namespace ZombieWar.Features.Control.View
{
    
    public sealed class DynamicJoystickView : MonoBehaviour, IControlView, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Coordinate Space")]
        [SerializeField] private RectTransform coordinateSpace;

        [Header("Joystick Visuals")]
        [SerializeField] private RectTransform joystickRoot;
        [SerializeField] private RectTransform handle;

        public event Action<ControlPointerSample> PointerDownRequested;
        public event Action<ControlPointerSample> PointerDragged;
        public event Action<int> PointerUpRequested;
        public event Action CancelRequested;

        private void Awake()
        {
            Hide();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                CancelRequested?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!TryConvert(eventData, out Vector2 localPosition))
                return;

            PointerDownRequested?.Invoke(new ControlPointerSample(eventData.pointerId, localPosition.x, localPosition.y));
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!TryConvert(eventData, out Vector2 localPosition))
                return;

            PointerDragged?.Invoke(new ControlPointerSample(eventData.pointerId, localPosition.x, localPosition.y));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUpRequested?.Invoke(eventData.pointerId);
        }

        public void ShowAt(float localX, float localY)
        {
            if (joystickRoot == null)
                return;

            joystickRoot.gameObject.SetActive(true);
            joystickRoot.anchoredPosition = new Vector2(localX, localY);
        }

        public void SetHandleOffset(float x, float y)
        {
            if (handle == null)
                return;

            handle.anchoredPosition = new Vector2(x, y);
        }

        public void Hide()
        {
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;

            if (joystickRoot != null)
                joystickRoot.gameObject.SetActive(false);
        }

        private bool TryConvert(PointerEventData eventData, out Vector2 localPosition)
        {
            localPosition = default;
            if (coordinateSpace == null)
                return false;

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(coordinateSpace, eventData.position, eventData.pressEventCamera, out localPosition);
        }
    }
}
