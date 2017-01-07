using System;
using System.Collections.Concurrent;

namespace CommandProcessor
{
    internal class QueuedCommandHandler : IDisposable
    {
        public Guid AggregateId { get; private set; }
        BlockingCollection<CommandTask> _queue;
        CommandHandler _commandHandler;

        public QueuedCommandHandler(Guid aggregateId, 
                                    CommandHandler handler)
        {
            AggregateId = aggregateId;
            _commandHandler = handler;
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

                bool isTryTakeSuccess = false;
                try
                {
                    isTryTakeSuccess = _queue.TryTake(out commandTask, 100); //Make timeout configurable?
                }catch(Exception e)
                {
                    throw e;
                }

                if (isTryTakeSuccess)
                {
                    try
                    {                     
                        _commandHandler.Handle(commandTask.Command);
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