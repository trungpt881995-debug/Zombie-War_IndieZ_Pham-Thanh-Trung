using System;

namespace GeneralCore.Architecture
{
    public interface IEvent { }

    public interface IEventBus
    {
        void Publish<TEvent>(TEvent evt) where TEvent : IEvent;
    }

    public interface IEventSubscriber
    {
        IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent;
    }
}
