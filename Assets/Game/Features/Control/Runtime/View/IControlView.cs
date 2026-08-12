using System;
using GeneralCore.Architecture;
using ZombieWar.Features.Control.Domain;

namespace ZombieWar.Features.Control.View
{
    public interface IControlView : IView
    {
        event Action<ControlPointerSample> PointerDownRequested;
        event Action<ControlPointerSample> PointerDragged;
        event Action<int> PointerUpRequested;
        event Action CancelRequested;

        void ShowAt(float localX, float localY);
        void SetHandleOffset(float x, float y);
        void Hide();
    }
}
