using BaseTypes;
using System;
using System.Threading.Tasks;

namespace CommandProcessor
{
    internal class CommandTask
    {
        public ICommand Command { get; private set; }
        private TaskCompletionSource<bool> _tcs;

        public CommandTask(ICommand command, TaskCompletionSource<bool> tcs)
        {
            Command = command;
            _tcs = tcs;
        }

        public void Finish(bool isCompleted)
        {
            _tcs.SetResult(isCompleted);
        }

        public void Finish(Exception exception)
        {
            _tcs.SetException(exception);
        }
    }
}