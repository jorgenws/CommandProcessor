using System;

namespace BaseTypes
{
    public interface ICommand
    {
        Guid AggregateId { get; }
    }
}