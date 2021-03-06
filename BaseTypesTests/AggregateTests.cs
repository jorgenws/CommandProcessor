﻿using BaseTypes;
using CommandProcessor;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BaseTypesTests
{
    [TestFixture]
    public class AggregateTests
    {
        private Mock<IEventStore> _eventStore;
        private Mock<ISnapshotRepository> _snapshotRepository;

        private Guid _aggregateId = Guid.Parse("{E4280787-CC29-4AF8-A77F-D2C20793F9E1}");
        private const string Value = "Test";

        [SetUp]
        public void SetUp()
        {
            _eventStore = new Mock<IEventStore>();
            _eventStore.Setup(c => c.WriteEvents(It.IsAny<List<IEvent>>()))
                       .Returns(new WriteEventsResult(true, 1));
            _snapshotRepository = new Mock<ISnapshotRepository>();
        }


        [Test]
        public void HandleOnCommandThatResultsInOneEvent()
        {
            var aggregate = CreateTestAggregate();

            
            var command = new TestCommand { AggregateId = _aggregateId, Value = Value };

            aggregate.Handle(command);

            Assert.AreEqual(Value, aggregate.Value);

            aggregate.Dispose();

            _eventStore.Verify(c => c.WriteEvents(It.IsAny<List<IEvent>>()), Times.Once);
        }

        [Test]
        public void ReplayOneCommand()
        {
            _eventStore.Setup(c => c.GetEventsForAggregate(_aggregateId))
                       .Returns(new[] { new TestEvent { AggragateId = _aggregateId, Value = Value } });

            var aggregate = CreateTestAggregate();

            aggregate.LoadState();

            Assert.AreEqual(Value, aggregate.Value);

        }

        private TestAggregate CreateTestAggregate()
        {
            var mapFactory = new HandlerMapFactory();
            var map = mapFactory.CreateFromAggregate(new[] { Assembly.GetExecutingAssembly() });
            var methods = map[typeof(TestAggregate)];

            var aggregate = new TestAggregate();
            aggregate.SetUp(_aggregateId, methods, _eventStore.Object, _snapshotRepository.Object);

            return aggregate;
        }
    }

    public class TestAggregate : Aggregate
    {
        public string Value { get; set; }

        public void Handle(TestCommand command)
        {
            Apply(new TestEvent { AggragateId = Id, Value = command.Value });
        }

        public void Handle(TestEvent @event)
        {
            Value = @event.Value;
        }
    }

    public class TestCommand : ICommand
    {
        public Guid AggregateId { get; set; }
        public string Value { get; set; }        
    }

    public class TestEvent : IEvent
    {
        public Guid AggragateId { get; set; }
        public string Value { get; set; }
    }
}
