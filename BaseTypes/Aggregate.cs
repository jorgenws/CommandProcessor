using System;
using System.Collections.ObjectModel;

namespace BaseTypes
{
    public abstract class Aggregate
    {
        public Guid Id { get; private set; }

        private ReadOnlyDictionary<Type, Action<object, object>> _handleMethods;

        internal void SetUp(Guid id, ReadOnlyDictionary<Type, Action<object, object>> handleMethods)
        {
            Id = id;
            _handleMethods = handleMethods;
        }

        internal void Handle(object @event)
        {
            if (_handleMethods.ContainsKey(@event.GetType()))
                _handleMethods[@event.GetType()](this, @event);
        }
    }

    public class AggregateFactory : IAggregateFactory
    {
        private IEventStore _eventStore;
        private ReadOnlyDictionary<Type,ReadOnlyDictionary<Type, Action<object,object>>> _handlerTypes;

        public AggregateFactory(IEventStore eventStore, 
                                ReadOnlyDictionary<Type, ReadOnlyDictionary<Type, Action<object,object>>> handlerTypes)
        {
            _eventStore = eventStore;
            _handlerTypes = handlerTypes;
        }

        public T Create<T>(Guid id) where T : Aggregate, new()
        {
            var aggregate = new T();
            var handleMethods = _handlerTypes[typeof(T)];
            aggregate.SetUp(id, handleMethods);

            //do eventstore query
            //replay events

            //add snapshotting?

            return aggregate;
        }
    }

    public interface IAggregateFactory
    {
        T Create<T>(Guid id) where T : Aggregate, new();
    }

    public interface IEvent { }

}
