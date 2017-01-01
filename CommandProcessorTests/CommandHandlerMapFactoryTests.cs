using BaseTypes;
using CommandProcessor;
using NUnit.Framework;
using System.Reflection;
using System;

namespace CommandProcessorTests
{
    [TestFixture]
    public class CommandHandlerMapFactoryTests
    {
        [Test]
        public void CreateDictionaryCommandToHandlerMap()
        {
            var factory = new CommandHandlerMapFactory();

            var commandHandlerMap = factory.Create(new[] { Assembly.GetExecutingAssembly() });

            Assert.IsTrue(commandHandlerMap.ContainsKey(typeof(TestCommand)));
        }

        public class TestCommandHandlerWithHandleMethodThatTakesAObjectOfICommand : ICommandHandler
        {
            public void Handle(TestCommand command)
            {
                //Not doing anything
            }
        }

        public class TestCommand : ICommand
        {
            public Guid AggregateId
            {
                get
                {
                    return Guid.NewGuid();
                }
            }
        }
    }
}
