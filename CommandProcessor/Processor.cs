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
        private ConcurrentDictionary<Guid, QueuedCommandHandler> _processingHandler;
        private ReadOnlyDictionary<Type, HandlerType> _commandHandlerMap;
        private IContainer _container;

        public Processor(ReadOnlyDictionary<Type, HandlerType> commandHandlerMap,
                         IContainer container)
        {
            _processingHandler = new ConcurrentDictionary<Guid, QueuedCommandHandler>();
            _commandHandlerMap = commandHandlerMap;
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
            while (!addingToExistingSuccess && !addingNewSuccess && addingFailureCounter < 5)
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
            if (_processingHandler.TryGetValue(commandTask.Command.AggregateId, out handler))
                return handler.Enqueue(commandTask);

            return false;
        }

        private bool TryAddingNewHandler(CommandTask commandTask)
        {
            var commandHandler = GetCommandHandler(commandTask.Command);
            var queuedCommandHandler = new QueuedCommandHandler(commandTask.Command.AggregateId, commandHandler);
            queuedCommandHandler.Enqueue(commandTask);
            if (_processingHandler.TryAdd(queuedCommandHandler.AggregateId, queuedCommandHandler))
                //Starting command processing
                Task.Factory.StartNew(() =>
                {
                    queuedCommandHandler.Run();
                    queuedCommandHandler.Dispose();

                    _processingHandler.TryRemove(commandTask.Command.AggregateId, out queuedCommandHandler);
                });
            else
                return false;

            return true;
        }

        private CommandHandler GetCommandHandler(ICommand command)
        {
            var commandType = command.GetType();

            if (!_commandHandlerMap.ContainsKey(commandType))
                throw new HandlerNotFoundException($"The handler for {commandType} is not found");

            var commandHandlerType = _commandHandlerMap[commandType];

            var commandHandler = (ICommandHandler)_container.Resolve(commandHandlerType.Handler);

            return new CommandHandler(commandHandler, commandHandlerType.HandleMethods);
        }

        public void Dispose()
        {
            //Look at killing the tasks if the timeout is exceded 
            //without the _proccessingHandler.IsEmpty returning true;

            const int MaxWaitTime = 1000;
            const int IntervalWaitTime = 10;
            int accumulatedWaitTime = 0;
            while (!_processingHandler.IsEmpty && accumulatedWaitTime < MaxWaitTime)
            {
                Task.Delay(IntervalWaitTime).Wait();
                accumulatedWaitTime += IntervalWaitTime;
            }
        }
    }

    public interface IProcessor : IDisposable
    {
        Task<bool> Process(ICommand command);
    }

    public class HandlerNotFoundException : Exception
    {
        public HandlerNotFoundException(string message) : base(message) { }
    }
}