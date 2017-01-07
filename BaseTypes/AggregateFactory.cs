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
        private ISnapshotFileNameBuilder _snapshotFileNameBuilder;
        private ReadOnlyDictionary<Type, ReadOnlyDictionary<Type, Action<object, object>>> _handlerTypes;

        public AggregateFactory(IComponentContext resolver,
                                IEventStore eventStore,
                                ISnapshotRepository snapshotRepository,
                                ISnapshotFileNameBuilder snapshotFileNameBuilder,
                                ReadOnlyDictionary<Type, ReadOnlyDictionary<Type, Action<object, object>>> handlerTypes)
        {
            _resolver = resolver;
            _eventStore = eventStore;
            _snapshotRepository = snapshotRepository;
            _snapshotFileNameBuilder = snapshotFileNameBuilder;
            _handlerTypes = handlerTypes;
        }

        public T Create<T>(Guid id) where T : Aggregate, new()
        {
            var aggregate = _resolver.Resolve<T>();
            var handleMethods = _handlerTypes[typeof(T)];
            aggregate.SetUp(id, handleMethods, _eventStore);

            if (aggregate is ISnapshot)
                LoadSnapshot(aggregate as ISnapshot);                

            foreach (IEvent @event in _eventStore.GetEventsForAggregate(id))
                aggregate.Handle(@event);
            
            return aggregate;
        }

        private void LoadSnapshot(ISnapshot snapshotAggregate)
        {
            var fileName = _snapshotFileNameBuilder.Build(snapshotAggregate.GetType(), snapshotAggregate.Id);
            var snapshot = _snapshotRepository.Load(fileName);
            snapshotAggregate.LoadSnapshot(snapshot);
        }
    }

    public interface IAggregateFactory
    {
        T Create<T>(Guid id) where T : Aggregate, new();
    }
}
