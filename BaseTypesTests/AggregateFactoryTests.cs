using System;
using System.Collections.ObjectModel;
using BaseTypes;
using NUnit.Framework;
using Moq;
using Autofac;
using CommandProcessor;
using System.Reflection;

namespace BaseTypesTests
{
    [TestFixture]
    public class AggregateFactoryTests
    {
        Mock<IEventStore> _eventStore;
        Mock<ISnapshotRepository> _snapshotRepository;

        readonly Guid _aggregateId = Guid.Parse("{EA201096-ADBF-4C38-985C-FC405C2C1795}");

        [SetUp]
        public void SetUp()
        {
            _eventStore = new Mock<IEventStore>();
            _snapshotRepository = new Mock<ISnapshotRepository>();
        }

        [Test]
        public void CreatesAggregateSetsItUpAndLoadTheState()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AggregateToValidateTest>().AsSelf();
            var container = builder.Build();
            

            var mapFactory = new HandlerMapFactory();
            var map = mapFactory.CreateFromAggregate(new[] { Assembly.GetExecutingAssembly() });

            var factory = new AggregateFactory(container, _eventStore.Object, _snapshotRepository.Object, map);

            var aggregate = factory.Create<AggregateToValidateTest>(_aggregateId);

            Assert.IsTrue(aggregate.SetUpWasRun);
            Assert.IsTrue(aggregate.LoadStateWasRun);
            Assert.AreEqual(_aggregateId, aggregate.Id);
        }

        public class AggregateToValidateTest : Aggregate
        {
            public bool SetUpWasRun { get; private set; }
            public bool LoadStateWasRun { get; private set; }

            internal override void SetUp(Guid id, ReadOnlyDictionary<Type, Action<object, object>> handleMethods, IEventStore eventStore, ISnapshotRepository snapshotRepository)
            {
                SetUpWasRun = true;
                base.SetUp(id, handleMethods, eventStore, snapshotRepository);
            }

            internal override void LoadState()
            {
                LoadStateWasRun = true;
                base.LoadState();
            }
        }
    }
}
