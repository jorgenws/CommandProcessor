using CommandProcessor;
using NUnit.Framework;

namespace CommandProcessorTests
{
    [TestFixture]
    public class CommandHandlerTests
    {
        [Test]
        public void CanProcessCommand()
        {
            var factory = new CommandHandlerMapFactory();
            var commandHandlerMap = factory.Create(new[] { typeof(TestHandlers.TestCommandHandler).Assembly });

            var commandHandlerType = commandHandlerMap[typeof(TestHandlers.TestCommand)];

            var concreteCommandHandler = new TestHandlers.TestCommandHandler();

            var handler = new CommandHandler(concreteCommandHandler, commandHandlerType.HandleMethods);

            var command = new TestHandlers.TestCommand();

            Assert.DoesNotThrow(() => handler.Handle(command));
        }
    }
}