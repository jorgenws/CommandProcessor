using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BaseTypes
{
    public abstract class Aggregate : IDisposable
    {
        public Guid Id { get; private set; }

        private ReadOnlyDictionary<Type, Action<object, object>> _handleMethods;
        private IEventStore _eventStore;
        private List<IEvent> _uncommitedEvents;

        internal void SetUp(Guid id, ReadOnlyDictionary<Type, Action<object, object>> handleMethods, IEventStore eventStore)
        {
            Id = id;
            _handleMethods = handleMethods;
            _eventStore = eventStore;
            _uncommitedEvents = new List<IEvent>();
        }

        internal void Handle(object @event)
        {
            if (_handleMethods.ContainsKey(@event.GetType()))
                _handleMethods[@event.GetType()](this, @event);
        }

        protected void Apply(IEvent @event)
        {
            Handle(@event);
            _uncommitedEvents.Add(@event);
        }

        public void Dispose()
        {
            var result = _eventStore.WriteEvents(_uncommitedEvents);
            if (!result.Item1)
                throw new EventStoreWriteException();
        }
    }
}