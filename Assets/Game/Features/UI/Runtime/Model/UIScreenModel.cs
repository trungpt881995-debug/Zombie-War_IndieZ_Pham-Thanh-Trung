using System;
using ZombieWar.Features.UI.Domain;
namespace ZombieWar.Features.UI.Model
{
    public sealed class UIScreenModel
    {
        public UIScreenId Current { get; private set; } = UIScreenId.None;
        public UIScreenId Previous { get; private set; } = UIScreenId.None;
        public event Action<UIScreenId> Changed;
        internal bool Set(UIScreenId next)
        {
            if (Current == next) return false;
            Previous = Current; Current = next; Changed?.Invoke(next); return true;
        }
    }
}
