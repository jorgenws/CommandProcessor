using Autofac;
using System;
using System.Collections.ObjectModel;

namespace BaseTypes
{
    public class AggregateFactory : IAggregateFactory
    {
        private IComponentContext _resolver;
        private IEventStore _eventStore;
        private ISnapshotRepository _snapshotRepository;
        private ReadOnlyDictionary<Type, ReadOnlyDictionary<Type, Action<object, object>>> _handlerTypes;

        public AggregateFactory(IComponentContext resolver,
                                IEventStore eventStore,
                                ISnapshotRepository snapshotRepository,
                                ReadOnlyDictionary<Type, ReadOnlyDictionary<Type, Action<object, object>>> handlerTypes)
        {
            _resolver = resolver;
            _eventStore = eventStore;
            _snapshotRepository = snapshotRepository;
            _handlerTypes = handlerTypes;
        }

        public T Create<T>(Guid id) where T : Aggregate, new()
        {
            var aggregate = _resolver.Resolve<T>();
            var handleMethods = _handlerTypes[typeof(T)];

            //I wish is had a better solution for this
            aggregate.SetUp(id, handleMethods, _eventStore, _snapshotRepository);

            aggregate.LoadState();

            return aggregate;
        }
    }

    public interface IAggregateFactory
    {
        T Create<T>(Guid id) where T : Aggregate, new();
    }
}
