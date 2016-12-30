using BaseTypes;
using System.Collections.Generic;
using System.Reflection;
using System;
using Autofac;
using System.Linq;

namespace CommandProcessor
{
    public class CommandProcessorBuilder : IAddAssemblies, ISetEventStore, ICommandProcessorBuilder
    {
        private IEnumerable<Assembly> _assemblies;
        private IEventStore _eventStore;

        public ISetEventStore AddAssemblies(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
            return this;
        }

        public ICommandProcessorBuilder SetEventStore(IEventStore eventStore)
        {
            _eventStore = eventStore;
            return this;
        }

        public IProcessor Build()
        {
            var builder = new ContainerBuilder();

            var mapFactory = new CommandToHandlerMapFactory();
            var commandHandlerMap = mapFactory.Create(_assemblies);

            var commandHandlers = commandHandlerMap.Values.Distinct();

            foreach (var commandHandler in commandHandlers)
                builder.RegisterType(commandHandler);

            var dependencies = DependenciesFactory.Create(_assemblies);

            foreach (var dependency in dependencies)
                dependency.Add(builder);

            //Look more into how to register the event store to make it available to be per call not a singleton.
            //builder.RegisterInstance<IEventStore>(_eventStore);

            var container = builder.Build();
          
            var processor = new Processor(commandHandlerMap, container);

            return processor;
        }
    }

    public interface IAddAssemblies
    {
         ISetEventStore AddAssemblies(IEnumerable<Assembly> assemblies);
    }

    public interface ISetEventStore
    {
         ICommandProcessorBuilder SetEventStore(IEventStore eventStore);
    }

    public interface ICommandProcessorBuilder
    {
        IProcessor Build();
    }
}
