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
            aggregate.SetUp(id, handleMethods);

            if (aggregate is ISnapshot)
                LoadSnapshot(aggregate as ISnapshot);
                

            foreach (IEvent @event in _eventStore.GetEventsForAggregate(id))
                aggregate.Handle(@event);
            
            return aggregate;
        }

        private void LoadSnapshot(ISnapshot snapshotAggregate)
        {
            string filename = $"{snapshotAggregate.Id.ToString("D")}-{typeof(ISnapshot)}"; //Move to a centeralized place to avoid duplication (handling retrival)       
            var snapshot = _snapshotRepository.Load(filename);
            snapshotAggregate.LoadSnapshot(snapshot);
        }
    }

    public interface IAggregateFactory
    {
        T Create<T>(Guid id) where T : Aggregate, new();
    }
}
