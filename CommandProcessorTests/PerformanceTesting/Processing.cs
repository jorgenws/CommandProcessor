using BaseTypes;
using CommandProcessor;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TestHandlers;

namespace CommandProcessorTests.PerformanceTesting
{
    [TestFixture]
    public class Processing
    {
        private readonly Guid _aggregateId1 = Guid.Parse("6C75E37D-E5D9-4679-9833-CD1621639AAD");
        private readonly Guid _aggregateId2 = Guid.Parse("{9DE9C3A7-A285-4937-8BB5-61280156CAC9}");
        private readonly Guid _aggregateId3 = Guid.Parse("{1F03EFE6-7323-4E63-971F-6A49D56A807E}");
        private readonly Guid _aggregateId4 = Guid.Parse("{7D4BBB90-8F7F-4DD9-AE2C-751E20430829}");

        private int numberOfCommands = 100000;

        [Test]
        public void ProcessingHundredThousandCommands()
        {
            var commands = new ICommand [] {
                new TestCommand { AggregateId = _aggregateId1 },
                new TestCommand2 { AggregateId = _aggregateId2 },
                new TestCommand { AggregateId = _aggregateId3 },
                new TestCommand2 { AggregateId = _aggregateId4 }
            };

            ICommandProcessorBuilder builder = new CommandProcessorBuilder();
            var processor = builder.AddAssemblies(new[] { typeof(TestHandlers.TestCommandHandler).Assembly })
                                   .SetEventStoreType<DummyEventStore>()
                                   .Build();
            
            var before = DateTime.UtcNow;
            ICommand command;
            var tasks = new ConcurrentBag<Task<bool>>();
            foreach (var i in Enumerable.Range(0, numberOfCommands))
            {
                command = commands[i % 4];
                tasks.Add(processor.Process(command));
            }

            Task.WhenAll(tasks.ToArray()).Wait();

            var success = tasks.Count(c => c.Result);
            var failure = tasks.Count(c => !c.Result);
            var exceptions = tasks.Count(c => c.Exception != null);

            var after = DateTime.UtcNow;

            var totalTime = (after - before).TotalMilliseconds;
            var rate = numberOfCommands / (after - before).TotalSeconds;

            Assert.Pass($"Total time is {totalTime} ms and the rate is {rate} commands per second. With {success} successfull commands, {failure} failed commands and {exceptions} exceptions");
        }
    }
}