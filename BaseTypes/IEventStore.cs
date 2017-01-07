using System;
using System.Collections.Generic;

namespace BaseTypes
{
    public interface IEventStore
    {
        Tuple<bool, int> WriteEvents(List<IEvent> events);
        IEnumerable<IEvent> GetEventsForAggregate(Guid aggregateId);
        IEnumerable<IEvent> GetEventsForAggregate(Guid aggregateId, int largerThan);
    }
}