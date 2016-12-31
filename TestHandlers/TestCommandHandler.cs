using BaseTypes;
using System;
using System.Threading.Tasks;

namespace TestHandlers
{
    public class TestCommandHandler : ICommandHandler
    {
        public void Handle(TestCommand command)
        {
            Task.Delay(200).Wait();
        }
    }

    public class TestCommand : ICommand
    {
        public Guid AggregateId { get; set; }
    }

    public class DummyEventStore : IEventStore { }
}
