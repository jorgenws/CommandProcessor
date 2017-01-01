using BaseTypes;
using System.Collections.Generic;
using System.Reflection;
using System;
using Autofac;
using System.Linq;

namespace CommandProcessor
{
    public class CommandProcessorBuilder : ICommandProcessorBuilder, ISetEventStore, ICommandProcessorBuild
    {
        private IEnumerable<Assembly> _assemblies;
        private Type _eventStore;
        private CommandHandlerMapFactory _commandHandlerMapFactory;
        private DependenciesFactory _dependenciesFactory;

        public CommandProcessorBuilder()
        {
            _commandHandlerMapFactory = new CommandHandlerMapFactory();
            _dependenciesFactory = new DependenciesFactory();

        }

        public ISetEventStore AddAssemblies(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
            return this;
        }

        public ICommandProcessorBuild SetEventStoreType<T>() where T : IEventStore
        {
            _eventStore = typeof(T);
            return this;
        }

        public IProcessor Build()
        {
            var builder = new ContainerBuilder();

            var commandHandlerMap = _commandHandlerMapFactory.Create(_assemblies);

            var commandHandlers = commandHandlerMap.Values.Select(c => c.CommandHandler)
                                                          .Distinct();

            foreach (var commandHandler in commandHandlers)
                builder.RegisterType(commandHandler).AsSelf();

            var dependencies = _dependenciesFactory.Create(_assemblies);

            foreach (var dependency in dependencies)
                dependency.Add(builder);

            builder.RegisterType(_eventStore).As<IEventStore>();

            var container = builder.Build();
          
            var processor = new Processor(commandHandlerMap, container);

            return processor;
        }
    }

    public interface ICommandProcessorBuilder
    {
         ISetEventStore AddAssemblies(IEnumerable<Assembly> assemblies);
    }

    public interface ISetEventStore
    {
         ICommandProcessorBuild SetEventStoreType<T>() where T : IEventStore;
    }

    public interface ICommandProcessorBuild
    {
        IProcessor Build();
    }
}
