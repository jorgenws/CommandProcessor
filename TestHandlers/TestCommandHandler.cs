using BaseTypes;
using System;
using System.Collections.Generic;

namespace TestHandlers
{
    public class TestCommandHandler : ICommandHandler
    {
        private readonly IAggregateFactory _aggregateFactory;

        public TestCommandHandler(IAggregateFactory aggregateFactory)
        {
            _aggregateFactory = aggregateFactory;
        }

        public void Handle(TestCommand command)
        {
            var aggregate = _aggregateFactory.Create<TestAggregate>(command.AggregateId);
            aggregate.Handle(command);
        }
    }

    public class TestCommand : ICommand
    {
        public Guid AggregateId { get; set; }
    }

    public class TestCommandHandler2 : ICommandHandler
    {
        public void Handle(TestCommand2 command)
        {
        }
    }

    public class TestCommand2 : ICommand
    {
        public Guid AggregateId { get; set; }
    }

    public class DummyEventStore : IEventStore
    {
        public IEnumerable<IEvent> GetEventsForAggregate(Guid aggregateId)
        {
            return new List<IEvent>();
        }

        public IEnumerable<IEvent> GetEventsForAggregate(Guid aggregateId, int largerThan)
        {
            return new List<IEvent>();
        }

        public Tuple<bool,int> WriteEvents(List<IEvent> events)
        {
            return new Tuple<bool, int>(true, 1);
        }
    }

    public class DummySnapshotRepository : ISnapshotRepository
    {
        public byte[] Load(string filename)
        {
            return new byte[0];
        }

        public bool Save(string filename, byte[] snapshot)
        {
            return true;
        }
    }

    public class TestAggregate : Aggregate
    {
        public void Handle(TestCommand command)
        {

        }

        public void Handle(TestEvent @event)
        {

        }
    }

    public class TestEvent : IEvent
    {
        public Guid AggragateId { get; }
    }
}
