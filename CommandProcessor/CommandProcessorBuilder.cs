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
        private Type _eventStore;

        public ISetEventStore AddAssemblies(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
            return this;
        }

        public ICommandProcessorBuilder SetEventStoreType<T>() where T : IEventStore
        {
            _eventStore = typeof(T);
            return this;
        }

        public IProcessor Build()
        {
            var builder = new ContainerBuilder();

            var mapFactory = new CommandToHandlerMapFactory();
            var commandHandlerMap = mapFactory.Create(_assemblies);

            var commandHandlers = commandHandlerMap.Values.Distinct();

            foreach (var commandHandler in commandHandlers)
                builder.RegisterType(commandHandler).AsSelf();

            var dependencies = DependenciesFactory.Create(_assemblies);

            foreach (var dependency in dependencies)
                dependency.Add(builder);

            builder.RegisterType(_eventStore).As<IEventStore>();

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
         ICommandProcessorBuilder SetEventStoreType<T>() where T : IEventStore;
    }

    public interface ICommandProcessorBuilder
    {
        IProcessor Build();
    }
}
