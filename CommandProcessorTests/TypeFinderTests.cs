using System;
using BaseTypes;
using CommandProcessor;
using NUnit.Framework;
using System.Reflection;
using System.Linq;

namespace CommandProcessorTests
{
    [TestFixture]
    public class TypeFinderTests
    {
        [Test]
        public void FindTypesImplementingInterface()
        {
            var finder = new TypeFinder();

            var types = finder.FindTypesOf<ICommandHandler>(new[] { Assembly.GetExecutingAssembly() });

            Assert.Contains(typeof(TestCommandHandler).GetTypeInfo(), types.ToArray());
        }

        [Test]
        public void FindTypesOfBaseClass()
        {
            var finder = new TypeFinder();

            var types = finder.FindTypesOf<TestCommandHandler>(new[] { Assembly.GetExecutingAssembly() });

            CollectionAssert.DoesNotContain(types.ToArray(), typeof(AbstractTestCommandHandler));
            Assert.Contains(typeof(TestCommandHandler).GetTypeInfo(), types.ToArray());
            Assert.Contains(typeof(SpesificTestCommandHandler).GetTypeInfo(), types.ToArray());
        }

        [Test]
        public void FindTypesImplementingInterfaceInBase()
        {
            var finder = new TypeFinder();

            var types = finder.FindTypesOf<ICommandHandler>(new[] { Assembly.GetExecutingAssembly() });

            CollectionAssert.DoesNotContain(types.ToArray(), typeof(AbstractTestCommandHandler));
            Assert.Contains(typeof(TestCommandHandler).GetTypeInfo(), types.ToArray());
            Assert.Contains(typeof(SpesificTestCommandHandler).GetTypeInfo(), types.ToArray());
        }
    }

    public abstract class AbstractTestCommandHandler : ICommandHandler { }

    public class TestCommandHandler : AbstractTestCommandHandler { }

    public class SpesificTestCommandHandler : TestCommandHandler
    {

    }
}
