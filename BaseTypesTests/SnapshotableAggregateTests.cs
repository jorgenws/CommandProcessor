using System;
using BaseTypes;
using NUnit.Framework;
using CommandProcessor;
using System.Reflection;
using Moq;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace BaseTypesTests
{
    [TestFixture]
    public class SnapshotableAggregateTests
    {
        private Guid _aggregate = Guid.Parse("{229834C8-E2DB-4AB4-9B20-424B85B3FD7C}");
        const string Value = "Test";

        private Mock<IEventStore> _eventStore;
        private Mock<ISnapshotRepository> _snapshotRepository;

        [SetUp]
        public void SetUp()
        {
            _eventStore = new Mock<IEventStore>();
            _snapshotRepository = new Mock<ISnapshotRepository>();
        }

        [Test]
        public void HandleOneCommandThatResultsInOneEvent()
        {
            _eventStore.Setup(c => c.WriteEvents(It.IsAny<List<IEvent>>()))
                       .Returns(new WriteEventsResult(true, 1));

            var aggregate = CreateSnapshotableAggregate();
            
            var command = new TestCommand { AggregateId = _aggregate, Value = Value };

            aggregate.Handle(command);

            Assert.AreEqual(Value, aggregate.Value);

            aggregate.Dispose();

            _eventStore.Verify(c => c.WriteEvents(It.IsAny<List<IEvent>>()), Times.Once);
        }

        [Test]
        public void ReplayOneCommand()
        {
            _snapshotRepository.Setup(c => c.Load(It.IsAny<string>()))
                               .Returns(new PersitableSnapshot { SnapshotFromId = -1, Snapshot = new byte[0] });

            _eventStore.Setup(c => c.GetEventsForAggregate(_aggregate))
                       .Returns(new[] { new TestEvent { AggragateId = _aggregate, Value = Value } });

            var aggregate = CreateSnapshotableAggregate();
            aggregate.LoadState();

            Assert.AreEqual(Value, aggregate.Value);
        }

        [Test]
        public void ReplayWithSnapshot()
        {
            byte[] valueAsBytes = Encoding.UTF8.GetBytes(Value);

            _snapshotRepository.Setup(c => c.Load(It.IsAny<string>()))
                   .Returns(new PersitableSnapshot { SnapshotFromId = 100, Snapshot = valueAsBytes });

            _eventStore.Setup(c => c.GetEventsForAggregate(_aggregate, 101))
                       .Returns(new TestEvent[0]);

            var aggregate = CreateSnapshotableAggregate();
            aggregate.LoadState();

            Assert.AreEqual(Value, aggregate.Value);
        }

        [Test]
        public void NeverSaveSnapshotWhenThereAreNoNewEvents()
        {
            _snapshotRepository.Setup(c => c.Load(It.IsAny<string>()))
                               .Returns(new PersitableSnapshot { SnapshotFromId = -1, Snapshot = new byte[0] });

            _eventStore.Setup(c => c.GetEventsForAggregate(_aggregate))
                       .Returns(Enumerable.Range(0, 1001).Select(c => new TestEvent { AggragateId = _aggregate, Value = Value }));

            _eventStore.Setup(c => c.WriteEvents(It.IsAny<List<IEvent>>()))
                       .Returns(new WriteEventsResult(true, -1));

            var aggregate = CreateSnapshotableAggregate();
            aggregate.LoadState();

            aggregate.Dispose();

            _snapshotRepository.Verify(c => c.Save(It.IsAny<string>(), It.IsAny<PersitableSnapshot>()), Times.Never);
        }

        [Test]
        public void SaveSnapshotWhenThereAreEnoughNewEvents()
        {
            byte[] valueAsByteArray = Encoding.UTF8.GetBytes(Value);
            
            _snapshotRepository.Setup(c => c.Load(It.IsAny<string>()))
                               .Returns(new PersitableSnapshot { SnapshotFromId = -1, Snapshot = new byte[0] });

            _eventStore.Setup(c => c.GetEventsForAggregate(_aggregate))
                       .Returns(Enumerable.Range(0, 1001).Select(c => new TestEvent { AggragateId = _aggregate, Value = Value }));

            _eventStore.Setup(c => c.WriteEvents(It.IsAny<List<IEvent>>()))
                       .Returns(new WriteEventsResult(true, 1002));

            var aggregate = CreateSnapshotableAggregate();
            aggregate.LoadState();

            var command = new TestCommand { AggregateId = _aggregate, Value = Value };
            aggregate.Handle(command);

            aggregate.Dispose();

            _snapshotRepository.Verify(c => c.Save(It.IsAny<string>(), It.Is<PersitableSnapshot>(d=>ByteArrayContentComparer(d.Snapshot, valueAsByteArray))), Times.Once);
        }

        private bool ByteArrayContentComparer(byte[] x, byte[] y)
        {
            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }

        private TestSnapshotableAggregate CreateSnapshotableAggregate()
        {
            var mapFactory = new HandlerMapFactory();
            var map = mapFactory.CreateFromAggregate(new[] { Assembly.GetExecutingAssembly() });
            var methods = map[typeof(TestSnapshotableAggregate)];

            var aggregate = new TestSnapshotableAggregate();
            aggregate.SetUp(_aggregate, methods, _eventStore.Object, _snapshotRepository.Object);

            return aggregate;
        }
    }

    public class TestSnapshotableAggregate : SnapshotableAggregate
    {
        public string Value { get; set; }

        public override void LoadFromSnapshot(byte[] snapshot)
        {
            Value = Encoding.UTF8.GetString(snapshot);
        }

        public override byte[] SaveAsSnapshot()
        {
            return Encoding.UTF8.GetBytes(Value);
        }

        public void Handle(TestCommand command)
        {
            Apply(new TestEvent { AggragateId = Id, Value = command.Value });
        }

        public void Handle(TestEvent @event)
        {
            Value = @event.Value;
        }
    }
}
