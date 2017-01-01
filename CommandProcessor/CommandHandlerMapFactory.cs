using BaseTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandProcessor
{
    internal class CommandHandlerMapFactory
    {
        private TypeFinder _finder;

        public CommandHandlerMapFactory()
        {
            _finder = new TypeFinder();
        }

        public ReadOnlyDictionary<Type, CommandHandlerType> Create(IEnumerable<Assembly> assemblies)
        {
            var map = _finder.FindTypesOf<ICommandHandler>(assemblies)
                             .SelectMany(c => GetCommandMethodParameters(c).Select(d => new { Command = d, Handler = new CommandHandlerType(c.AsType(), new ReadOnlyDictionary<Type, Action<object, object>>(CreateHandlerDelegates(c.AsType()))) }))
                             .ToDictionary(c => c.Command, c => c.Handler);
            return new ReadOnlyDictionary<Type, CommandHandlerType>(map);
        }

        private IEnumerable<Type> GetCommandMethodParameters(TypeInfo commandHandler)
        {
            return commandHandler.DeclaredMethods.Where(c => c.IsPublic &&
                                                             c.Name == "Handle" &&
                                                             c.GetParameters().Length == 1 &&
                                                             c.GetParameters().First().ParameterType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ICommand)))
                                                 .Select(c => c.GetParameters().First().ParameterType.GetTypeInfo().AsType());
        }

        private Dictionary<Type, Action<object, object>> CreateHandlerDelegates(Type commandHandlerType)
        {
            return commandHandlerType.GetTypeInfo().DeclaredMethods.Where(c => c.Name == "Handle" &&
                                                                               c.IsPublic &&
                                                                               c.GetParameters().Length == 1 &&
                                                                               c.GetParameters().First().ParameterType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ICommand)))
                                                                    .Select(c => new { Command = c.GetParameters().First().ParameterType, HandleMethod = CreateHandleDelegate(commandHandlerType, c) })
                                                                    .ToDictionary(c => c.Command, c => c.HandleMethod);
        }

        private Action<object, object> CreateHandleDelegate(Type commandHandlerType, MethodInfo handle)
        {
            var parameterType = handle.GetParameters().First().ParameterType;

            var handler = Expression.Parameter(typeof(object), "handler");
            var argument = Expression.Parameter(typeof(object), "command");

            var methodCall = Expression.Call(Expression.Convert(handler, commandHandlerType),
                                             handle,
                                             Expression.Convert(argument, parameterType));

            return Expression.Lambda<Action<object, object>>(methodCall, handler, argument).Compile();
        }

    }

    internal class CommandHandlerType
    {
        public Type CommandHandler { get; private set; }
        public ReadOnlyDictionary<Type, Action<object, object>> HandleMethods { get; private set; }

        public CommandHandlerType(Type commandHandler,
                                  ReadOnlyDictionary<Type, Action<object, object>> handleMethods)
        {
            CommandHandler = commandHandler;
            HandleMethods = handleMethods;
        }
    }
}
