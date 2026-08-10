using System;
using System.Collections.Generic;

namespace GeneralCore.PerformanceMemory
{
    public sealed class ObjectPool<T> : IPool<T>
    {
        private readonly Stack<T> _items;
        private readonly Func<T> _factory;
        private readonly Action<T> _onRent;
        private readonly Action<T> _onReturn;
        private readonly int _maxSize;

        public ObjectPool(Func<T> factory, Action<T> onRent = null, Action<T> onReturn = null, int initialCapacity = 0, int maxSize = 256)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _onRent = onRent;
            _onReturn = onReturn;
            _maxSize = Math.Max(1, maxSize);
            _items = new Stack<T>(Math.Max(0, initialCapacity));
            for (var i = 0; i < initialCapacity; i++) _items.Push(_factory());
        }

        public int CountInactive => _items.Count;

        public T Rent()
        {
            var item = _items.Count > 0 ? _items.Pop() : _factory();
            _onRent?.Invoke(item);
            return item;
        }

        public void Return(T item)
        {
            _onReturn?.Invoke(item);
            if (_items.Count < _maxSize) _items.Push(item);
        }
    }
}
