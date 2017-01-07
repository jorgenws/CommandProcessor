using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BaseTypes
{
    public abstract class SnapshotableAggregate : Aggregate
    {
        private ISnapshotRepository _snapshotRepository;

        private int _numberOfEventsLoaded = 0;

        internal void SetUp(Guid id, 
                            ReadOnlyDictionary<Type, Action<object, object>> handleMethods, 
                            IEventStore eventStore, 
                            ISnapshotRepository snapshotRepository)
        {
            _snapshotRepository = snapshotRepository;
            SetUp(id, handleMethods, eventStore);
        }

        internal override void LoadState()
        {
            Tuple<byte[], int> snapshotAndHighestEventId = _snapshotRepository.Load(BuildSnapshotName());
            LoadFromSnapshot(snapshotAndHighestEventId.Item1);

            foreach (var @event in _eventStore.GetEventsForAggregate(Id, snapshotAndHighestEventId.Item2))
            {
                Handle(@event);
                _numberOfEventsLoaded++;
            }
        }

        public abstract void LoadFromSnapshot(byte[] snapshot);
        public abstract byte[] SaveAsSnapshot();

        public override void Dispose()
        {
            base.Dispose();

            if (_numberOfEventsLoaded > 1000)
            {
                byte[] snapshot = SaveAsSnapshot();
                _snapshotRepository.Save(BuildSnapshotName(), snapshot);
            }
        }

        private string BuildSnapshotName()
        {
            return $"{GetType()}-{Id.ToString("D")}";
        }
    }

    public abstract class Aggregate : IDisposable
    {
        public Guid Id { get; private set; }

        private ReadOnlyDictionary<Type, Action<object, object>> _handleMethods;
        internal IEventStore _eventStore;
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

        internal virtual void LoadState()
        {
            foreach (IEvent @event in _eventStore.GetEventsForAggregate(Id))
                Handle(@event);
        }

        public virtual void Dispose()
        {
            var result = _eventStore.WriteEvents(_uncommitedEvents);
            if (!result.Item1)
                throw new EventStoreWriteException();            
        }
    }
}