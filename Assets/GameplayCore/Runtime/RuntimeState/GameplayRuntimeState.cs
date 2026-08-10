using System;
using System.Collections.Generic;

namespace GameplayCore.RuntimeState
{
    public interface IGameplayRuntimeState
    {
        void Set<T>(string key, T value);
        bool TryGet<T>(string key, out T value);
        void Clear();
    }

    public sealed class GameplayRuntimeState : IGameplayRuntimeState
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public void Set<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));
            _values[key] = value;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (_values.TryGetValue(key, out var raw) && raw is T typed) { value = typed; return true; }
            value = default;
            return false;
        }

        public void Clear() => _values.Clear();
    }
}
