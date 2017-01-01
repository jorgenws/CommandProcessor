using BaseTypes;
using System;
using System.Collections.ObjectModel;

namespace CommandProcessor
{
    internal class CommandHandler
    {
        private ICommandHandler _commandHandler;
        private ReadOnlyDictionary<Type, Action<object, object>> _handleMethods;

        public CommandHandler(ICommandHandler commandHandler,
                              ReadOnlyDictionary<Type, Action<object,object>> handleMethods)
        {
            _commandHandler = commandHandler;
            _handleMethods = handleMethods;
        }

        public void Handle(ICommand command)
        {
            var handle = _handleMethods[command.GetType()];
            handle(_commandHandler, command);
        }
    }
}
