using BaseTypes;
using CommandProcessor;
using Moq;
using NUnit.Framework;
using TestHandlers;

namespace CommandProcessorTests
{
    [TestFixture]
    public class CommandHandlerTests
    {
        private Mock<IAggregateFactory> _aggregateFactory;

        [Test]
        public void CanProcessCommand()
        {
            var command = new TestCommand();

            _aggregateFactory = new Mock<IAggregateFactory>();
            _aggregateFactory.Setup(c => c.Create<TestAggregate>(command.AggregateId))
                .Returns(new TestAggregate());

            var factory = new HandlerMapFactory();
            var commandHandlerMap = factory.CreateFromCommandHandler(new[] { typeof(TestHandlers.TestCommandHandler).Assembly });

            var commandHandlerType = commandHandlerMap[typeof(TestCommand)];

            var concreteCommandHandler = new TestHandlers.TestCommandHandler(_aggregateFactory.Object);

            var handler = new CommandHandler(concreteCommandHandler, commandHandlerType.HandleMethods);

            

            Assert.DoesNotThrow(() => handler.Handle(command));
        }
    }
}