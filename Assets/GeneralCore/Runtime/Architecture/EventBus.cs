using System;
using System.Collections.Generic;

namespace GeneralCore.Architecture
{
    public sealed class EventBus : IEventBus, IEventSubscriber
    {
        private readonly Dictionary<Type, Delegate> _routes = new Dictionary<Type, Delegate>();

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            var type = typeof(TEvent);
            _routes.TryGetValue(type, out var current);
            _routes[type] = Delegate.Combine(current, handler);
            return new Subscription(() => Unsubscribe(handler));
        }

        public void Publish<TEvent>(TEvent evt) where TEvent : IEvent
        {
            if (_routes.TryGetValue(typeof(TEvent), out var current))
                ((Action<TEvent>)current)?.Invoke(evt);
        }

        private void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            var type = typeof(TEvent);
            if (!_routes.TryGetValue(type, out var current)) return;
            var next = Delegate.Remove(current, handler);
            if (next == null) _routes.Remove(type);
            else _routes[type] = next;
        }

        private sealed class Subscription : IDisposable
        {
            private Action _dispose;
            public Subscription(Action dispose) => _dispose = dispose;
            public void Dispose()
            {
                var action = _dispose;
                _dispose = null;
                action?.Invoke();
            }
        }
    }
}
