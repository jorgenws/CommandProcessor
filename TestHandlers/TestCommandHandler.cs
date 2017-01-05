using BaseTypes;
using System;

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

    public class DummyEventStore : IEventStore { }

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

    }
}
