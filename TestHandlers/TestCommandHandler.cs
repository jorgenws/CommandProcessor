using BaseTypes;
using System;

namespace TestHandlers
{
    public class TestCommandHandler : ICommandHandler
    {
        public void Handle(TestCommand command)
        {
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
}
