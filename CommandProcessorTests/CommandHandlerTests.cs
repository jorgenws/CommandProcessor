using BaseTypes;
using CommandProcessor;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TestHandlers;

namespace CommandProcessorTests
{
    [TestFixture]
    public class CommandHandlerTests
    {
        private Mock<IAggregateFactory> _aggregateFactory;
        private Mock<IEventStore> _eventStore;
        private Mock<ISnapshotRepository> _snapshotRepository;

        [Test]
        public void CanProcessCommand()
        {
            var command = new TestCommand();

            _snapshotRepository = new Mock<ISnapshotRepository>();

            _eventStore = new Mock<IEventStore>();
            _eventStore.Setup(c => c.WriteEvents(It.IsAny<List<IEvent>>()))
                       .Returns(new WriteEventsResult(true, 1));

            var testAggregate = new TestAggregate();
            testAggregate.SetUp(command.AggregateId, 
                                new ReadOnlyDictionary<Type, Action<object, object>>(new Dictionary<Type, Action<object, object>>()), 
                                _eventStore.Object, 
                                _snapshotRepository.Object);

            _aggregateFactory = new Mock<IAggregateFactory>();
            _aggregateFactory.Setup(c => c.Create<TestAggregate>(command.AggregateId))
                             .Returns(testAggregate);

            var factory = new HandlerMapFactory();
            var commandHandlerMap = factory.CreateFromCommandHandler(new[] { typeof(TestHandlers.TestCommandHandler).Assembly });

            var commandHandlerType = commandHandlerMap[typeof(TestCommand)];

            var concreteCommandHandler = new TestHandlers.TestCommandHandler(_aggregateFactory.Object);

            var handler = new CommandHandler(concreteCommandHandler, commandHandlerType.HandleMethods);        

            Assert.DoesNotThrow(() => handler.Handle(command));
        }
    }
}