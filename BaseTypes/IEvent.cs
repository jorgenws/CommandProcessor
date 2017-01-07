using System;

namespace BaseTypes
{
    public interface IEvent
    {
        Guid AggragateId { get; }
    }
}
