using BaseTypes;
using System.Collections.Generic;
using System.Reflection;
using System;
using Autofac;
using System.Linq;

namespace CommandProcessor
{
    public class CommandProcessorBuilder : ICommandProcessorBuilder, ISetEventStore, ISetSnapshotRepository, ISetFileSnapshotConfiguration, ICommandProcessorBuild
    {
        private IEnumerable<Assembly> _assemblies;
        private Type _eventStore;
        private Type _snapshotRepository;
        private string _fileSnapshotConfiguration;
        private HandlerMapFactory _handlerMapFactory;
        private DependenciesFactory _dependenciesFactory;

        public CommandProcessorBuilder()
        {
            _handlerMapFactory = new HandlerMapFactory();
            _dependenciesFactory = new DependenciesFactory();
        }

        public ISetEventStore AddAssemblies(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
            return this;
        }

        public ISetSnapshotRepository SetEventStoreType<T>() where T : IEventStore
        {
            _eventStore = typeof(T);
            return this;
        }

        public ICommandProcessorBuild SetSnapshotRepository<T>() where T : ISnapshotRepository
        {
            _snapshotRepository = typeof(T);
            return this;
        }

        public ISetFileSnapshotConfiguration SetFileSnapshotRepository()
        {
            _snapshotRepository = typeof(FileSnapshotRepository);
            return this;
        }

        public ICommandProcessorBuild Configuration(string fileSnapshotPath)
        {
            _fileSnapshotConfiguration = fileSnapshotPath;
            return this;
        }

        public IProcessor Build()
        {
            var builder = new ContainerBuilder();

            var commandHandlerMap = _handlerMapFactory.CreateFromCommandHandler(_assemblies);
            var commandHandlers = commandHandlerMap.Values.Select(c => c.Handler)
                                                          .Distinct();

            foreach (var commandHandler in commandHandlers)
                builder.RegisterType(commandHandler).AsSelf();

            var eventHandlerMap = _handlerMapFactory.CreateFromAggregate(_assemblies);

            var aggregates = eventHandlerMap.Select(c => c.Key);
            foreach (var aggregate in aggregates)
                builder.RegisterType(aggregate);

            builder.RegisterInstance(eventHandlerMap).AsSelf();

            var dependencies = _dependenciesFactory.Create(_assemblies);

            foreach (var dependency in dependencies)
                dependency.Add(builder);

            builder.RegisterType<AggregateFactory>().As<IAggregateFactory>();
            builder.RegisterType(_eventStore).As<IEventStore>();
            builder.RegisterType(_snapshotRepository).As<ISnapshotRepository>();
            builder.RegisterInstance(new FileSnapshotRepositoryConfiguration(_fileSnapshotConfiguration)).AsSelf().SingleInstance();

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
         ISetSnapshotRepository SetEventStoreType<T>() where T : IEventStore;
    }

    public interface ISetSnapshotRepository
    {
        ISetFileSnapshotConfiguration SetFileSnapshotRepository();
        ICommandProcessorBuild SetSnapshotRepository<T>() where T : ISnapshotRepository;
    }

    public interface ISetFileSnapshotConfiguration
    {
        ICommandProcessorBuild Configuration(string fileSnapshotPath);
    }

    public interface ICommandProcessorBuild
    {
        IProcessor Build();
    }
}
