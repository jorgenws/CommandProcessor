using BaseTypes;
using NUnit.Framework;
using System.Linq;
using Autofac;
using CommandProcessor;
using System.Reflection;

namespace CommandProcessorTests
{
    [TestFixture]
    public class DependenciesFactoryTests
    {
        [Test]
        public void CreateDependencyFromAssembly()
        {
            var factory = new DependenciesFactory();
            var dependencies = factory.Create(new[] { Assembly.GetExecutingAssembly() });

            var dependency = dependencies.First();

            Assert.IsInstanceOf<TestDependency>(dependency);
        }
    }

    public class TestDependency : IDependencies
    {
        public void Add(ContainerBuilder builder)
        {            
        }
    }
}
