using System;
using System.Collections.Generic;

namespace BaseTypes
{
    public interface IEventStore
    {
        WriteEventsResult WriteEvents(List<IEvent> events);
        IEnumerable<IEvent> GetEventsForAggregate(Guid aggregateId);
        IEnumerable<IEvent> GetEventsForAggregate(Guid aggregateId, int largerThan);
    }

    public class WriteEventsResult
    {
        public bool Success { get; private set; }
        public int LastWrittenEventId { get; private set; }

        public WriteEventsResult(bool success, int lastWrittenEventId)
        {
            Success = success;
            LastWrittenEventId = lastWrittenEventId;
        }
    }
}