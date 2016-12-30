using BaseTypes;
using CommandProcessor;
using NUnit.Framework;
using System.Reflection;
using System;

namespace CommandProcessorTests
{
    [TestFixture]
    public class CommandToHandlerMapFactoryTests
    {
        [Test]
        public void CreateDictionaryCommandToHandlerMap()
        {
            var factory = new CommandToHandlerMapFactory();

            var commandToHandlerMap = factory.Create(new[] { Assembly.GetExecutingAssembly() });

            Assert.IsTrue(commandToHandlerMap.ContainsKey(typeof(TestCommand)));
            Assert.AreEqual(typeof(TestCommandHandlerWithHandleMethodThatTakesAObjectOfICommand), commandToHandlerMap[typeof(TestCommand)]);
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
