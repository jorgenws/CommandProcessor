using Autofac;
using BaseTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CommandProcessor
{
    internal class Processor : IProcessor
    {
        //TODO:support looking up a combination of handler id and handler type if the same id is used multiple places
        private ConcurrentDictionary<Guid, QueuedCommandHandler> _processingAggregates;
        private ReadOnlyDictionary<Type, Type> _commandHandlerToCommandMap;
        private IContainer _container;

        public Processor(ReadOnlyDictionary<Type,Type> commandHandlerToCommandMap,
                         IContainer container)
        {
            _processingAggregates = new ConcurrentDictionary<Guid, QueuedCommandHandler>();
            _commandHandlerToCommandMap = commandHandlerToCommandMap;
            _container = container;
        }

        public Task<bool> Process(ICommand command)
        {
            var tcs = new TaskCompletionSource<bool>();
            var commandTask = new CommandTask(command, tcs);

            //Trying to add the command five times. 
            //If it is not added by then we give up and the user must retry or handle it.
            int addingFailureCounter = 0;
            bool addingToExistingSuccess = false;
            bool addingNewSuccess = false;
            while (!addingToExistingSuccess && !addingNewSuccess && addingFailureCounter > 5)
            {
                addingToExistingSuccess = TryAddingToExisitingHandler(commandTask);
                if(!addingToExistingSuccess)
                    addingNewSuccess = TryAddingNewHandler(commandTask);

                if (!addingToExistingSuccess & !addingNewSuccess)
                    addingFailureCounter++;
            }

            return tcs.Task;
        }

        private bool TryAddingToExisitingHandler(CommandTask commandTask)
        {
            QueuedCommandHandler handler;
            if (_processingAggregates.TryGetValue(commandTask.Command.AggregateId, out handler))
                return handler.Enqueue(commandTask);

            return false;
        }

        private bool TryAddingNewHandler(CommandTask commandTask)
        {
            var commandHandler = GetCommandHandler(commandTask.Command);
            var queuedCommandHandler = new QueuedCommandHandler(commandTask.Command.AggregateId, commandHandler);
            queuedCommandHandler.Enqueue(commandTask);
            if (_processingAggregates.TryAdd(queuedCommandHandler.AggregateId, queuedCommandHandler))
                //Starting command processing
                Task.Factory.StartNew(() =>
                {
                    queuedCommandHandler.Run();
                    queuedCommandHandler.Dispose();

                    _processingAggregates.TryRemove(commandTask.Command.AggregateId, out queuedCommandHandler);
                });
            else
                return false;

            return true;
        }

        private ICommandHandler GetCommandHandler(ICommand command)
        {
            var commandType = command.GetType();

            if (!_commandHandlerToCommandMap.ContainsKey(commandType))
                throw new HandlerNotFoundException($"The handler for {commandType} is not found");

            var commandHandlerType = _commandHandlerToCommandMap[commandType];

            return (ICommandHandler) _container.Resolve(commandHandlerType);
        }
    }

    public interface IProcessor
    {
        Task<bool> Process(ICommand command);
    }

    public class HandlerNotFoundException : Exception
    {
        public HandlerNotFoundException(string message) : base(message) { }
    }
}