using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BaseTypes
{


    public abstract class Aggregate : IDisposable
    {
        public Guid Id { get; private set; }

        private ReadOnlyDictionary<Type, Action<object, object>> _handleMethods;
        internal IEventStore _eventStore;
        private List<IEvent> _uncommitedEvents;

        internal virtual void SetUp(Guid id, 
                                    ReadOnlyDictionary<Type, Action<object, object>> handleMethods, 
                                    IEventStore eventStore, 
                                    ISnapshotRepository snapshotRepository)
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

        internal virtual void LoadState()
        {
            foreach (IEvent @event in _eventStore.GetEventsForAggregate(Id))
                Handle(@event);
        }

        public virtual void Dispose()
        {
            WriteEvents();
        }

        internal int WriteEvents()
        {
            //There is nothing to write
            if (!_uncommitedEvents.Any())
                return -1;

            var result = _eventStore.WriteEvents(_uncommitedEvents);
            if (result.Success)
            {
                _uncommitedEvents.Clear();
                return result.LastWrittenEventId;
            }
            else
                throw new EventStoreWriteException();
        }
    }
}