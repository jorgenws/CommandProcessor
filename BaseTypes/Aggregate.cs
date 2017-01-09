using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BaseTypes
{
    public abstract class SnapshotableAggregate : Aggregate
    {
        private ISnapshotRepository _snapshotRepository;

        private int _numberOfEventsLoaded = 0;

        internal override void SetUp(Guid id, 
                                     ReadOnlyDictionary<Type, Action<object, object>> handleMethods, 
                                     IEventStore eventStore, 
                                     ISnapshotRepository snapshotRepository)
        {
            _snapshotRepository = snapshotRepository;
            base.SetUp(id, handleMethods, eventStore, snapshotRepository);
        }

        internal override void LoadState()
        {
            PersitableSnapshot snapshotAndHighestEventId = _snapshotRepository.Load(BuildSnapshotName());
            LoadFromSnapshot(snapshotAndHighestEventId.Snapshot);

            foreach (var @event in _eventStore.GetEventsForAggregate(Id, snapshotAndHighestEventId.SnapshotFromId)) //id +1?
            {
                Handle(@event);
                _numberOfEventsLoaded++;
            }
        }

        public abstract void LoadFromSnapshot(byte[] snapshot);
        public abstract byte[] SaveAsSnapshot();

        public override void Dispose()
        {
            var lastWrittenEventId = WriteEvents();

            if (_numberOfEventsLoaded > 1000) //config?
            {
                byte[] snapshot = SaveAsSnapshot();
                var persistableSnapshot = new PersitableSnapshot { SnapshotFromId = lastWrittenEventId, Snapshot = snapshot };
                _snapshotRepository.Save(BuildSnapshotName(), persistableSnapshot);
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