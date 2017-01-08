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
            using (var aggregate = _aggregateFactory.Create<TestAggregate>(command.AggregateId))
            {
                aggregate.Handle(command);
            }            
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

        public WriteEventsResult WriteEvents(List<IEvent> events)
        {
            return new WriteEventsResult(true, 0);
        }
    }

    public class DummySnapshotRepository : ISnapshotRepository
    {
        public PersitableSnapshot Load(string filename)
        {
            return new PersitableSnapshot { SnapshotFromId = 0, Snapshot = new byte[0] };
        }

        public bool Save(string filename, PersitableSnapshot snapshotResult)
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
