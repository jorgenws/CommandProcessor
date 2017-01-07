using CommandProcessor;
using NUnit.Framework;
using TestHandlers;

namespace CommandProcessorTests
{
    [TestFixture]
    public class ProcessorTests
    {
        [Test]
        public void ProcessOneCommand()
        {
            var assembly = typeof(TestHandlers.TestCommandHandler).Assembly;

            ICommandProcessorBuilder builder = new CommandProcessorBuilder();

            var processor = builder.AddAssemblies(new[] { assembly })
                            .SetEventStoreType<DummyEventStore>()
                            .SetSnapshotRepository<DummySnapshotRepository>()
                            .Build();

            var command = new TestCommand();
            var commandTask = processor.Process(command);

            commandTask.Wait();

            processor.Dispose();

            Assert.IsTrue(commandTask.Result);
        }
    }
}