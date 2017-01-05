using Autofac;
using System;
using System.Collections.ObjectModel;

namespace BaseTypes
{
    public class AggregateFactory : IAggregateFactory
    {
        private IComponentContext _resolver;
        private IEventStore _eventStore;
        private ReadOnlyDictionary<Type, ReadOnlyDictionary<Type, Action<object, object>>> _handlerTypes;

        public AggregateFactory(IComponentContext resolver,
                                IEventStore eventStore,
                                ReadOnlyDictionary<Type, ReadOnlyDictionary<Type, Action<object, object>>> handlerTypes)
        {
            _resolver = resolver;
            _eventStore = eventStore;
            _handlerTypes = handlerTypes;
        }

        public T Create<T>(Guid id) where T : Aggregate, new()
        {
            //Use container to create Aggregate?
            //var aggregate = new T();
            var aggregate = _resolver.Resolve<T>();
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
}
