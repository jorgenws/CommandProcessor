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
            IEnumerable<IEvent> _events;
            PersitableSnapshot snapshotAndHighestEventId = _snapshotRepository.Load(BuildSnapshotName());
            if (snapshotAndHighestEventId.SnapshotFromId != -1)
            {
                LoadFromSnapshot(snapshotAndHighestEventId.Snapshot);
                _events = _eventStore.GetEventsForAggregate(Id, snapshotAndHighestEventId.SnapshotFromId + 1);
            }
            else
            {
                _events = _eventStore.GetEventsForAggregate(Id);
            }

            foreach (var @event in _events)
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
            if (lastWrittenEventId == -1) //Nothing has changed since the last write
                return;

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
}
