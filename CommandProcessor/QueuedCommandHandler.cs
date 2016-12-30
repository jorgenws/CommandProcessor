using BaseTypes;
using System;
using System.Collections.Concurrent;

namespace CommandProcessor
{
    internal class QueuedCommandHandler : IDisposable
    {
        public Guid AggregateId { get; private set; }
        BlockingCollection<CommandTask> _queue;
        ICommandHandler _handler;

        public QueuedCommandHandler(Guid aggregateId, 
                                    ICommandHandler handler)
        {
            AggregateId = aggregateId;
            _handler = handler;
            _queue = new BlockingCollection<CommandTask>();
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
                        //_handler.Handle(command); Will handle this later... HAHAHA!!!
                        commandTask.Finish(true); //get more feedback from handler?
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