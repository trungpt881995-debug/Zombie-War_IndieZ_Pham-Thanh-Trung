using System;
using System.Collections.Generic;

namespace GeneralCore.Architecture
{
    public sealed class CommandBus : ICommandBus, ICommandRegistry
    {
        private readonly Dictionary<Type, object> _handlers = new Dictionary<Type, object>();

        public void Register<TCommand>(ICommandHandler<TCommand> handler) where TCommand : ICommand
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _handlers[typeof(TCommand)] = handler;
        }

        public void Unregister<TCommand>(ICommandHandler<TCommand> handler) where TCommand : ICommand
        {
            if (_handlers.TryGetValue(typeof(TCommand), out var current) && ReferenceEquals(current, handler))
                _handlers.Remove(typeof(TCommand));
        }

        public void Send<TCommand>(TCommand command) where TCommand : ICommand
        {
            if (!_handlers.TryGetValue(typeof(TCommand), out var rawHandler))
                throw new InvalidOperationException($"No command handler registered for {typeof(TCommand).FullName}.");

            ((ICommandHandler<TCommand>)rawHandler).Handle(command);
        }
    }
}
