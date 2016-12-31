using BaseTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandProcessor
{
    internal class QueuedCommandHandler : IDisposable
    {
        public Guid AggregateId { get; private set; }
        BlockingCollection<CommandTask> _queue;
        ICommandHandler _handler;

        Dictionary<Type, Action<object, object>> _handlers;

        public QueuedCommandHandler(Guid aggregateId, 
                                    ICommandHandler handler)
        {
            AggregateId = aggregateId;
            _handler = handler;
            _queue = new BlockingCollection<CommandTask>();
            _handlers = new Dictionary<Type, Action<object, object>>();
            CreateHandlerDelegates();
        }

        private void CreateHandlerDelegates()
        {

            var handlerType = _handler.GetType();

            var handleMethods = handlerType.GetTypeInfo().DeclaredMethods.Where(c => c.Name == "Handle" &&
                                                                                     c.IsPublic &&
                                                                                     c.GetParameters().Length == 1 &&
                                                                                     c.GetParameters().First().ParameterType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ICommand)));

            foreach(var handle in handleMethods)
            {
                var parameterType = handle.GetParameters().First().ParameterType;

                var handler = Expression.Parameter(typeof(object), "handler");
                var argument = Expression.Parameter(typeof(object), "command");

                var methodCall = Expression.Call(Expression.Convert(handler, handlerType),
                                                 handle,
                                                 Expression.Convert(argument, parameterType));

                Action<object,object> action = Expression.Lambda<Action<object, object>>(methodCall, handler, argument).Compile();

                _handlers.Add(parameterType, action);
            }
        }
             
        public bool Enqueue(CommandTask command)
        {
            bool success = false;
            while (!success && !_queue.IsAddingCompleted)
            {
                try
                {
                    success = _queue.TryAdd(command, 10);
                }
                catch (InvalidOperationException)
                {
                    success = false;
                    break;
                }
            }

            return success;
        }

        public void Run()
        {
            while (!_queue.IsCompleted)
            {
                CommandTask commandTask;
                bool isTryTakeSuccess = _queue.TryTake(out commandTask, 100); //Make timeout configurable?

                if (isTryTakeSuccess)
                {
                    try
                    {
                        var command = commandTask.Command;
                        Action<object, object> handle = _handlers[command.GetType()];
                        handle(_handler, command);
                        commandTask.Finish(true); 
                    }
                    catch (Exception e)
                    {
                        commandTask.Finish(e);
                    }
                }
                else
                    _queue.CompleteAdding();
            }
        }

        public void Dispose()
        {
            _queue.Dispose();
        }
    }
}