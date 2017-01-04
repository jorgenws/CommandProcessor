using BaseTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandProcessor
{ 
    internal class HandlerMapFactory
    {
        private TypeFinder _finder;

        public HandlerMapFactory()
        {
            _finder = new TypeFinder();
        }

        public ReadOnlyDictionary<Type, HandlerType> CreateFromCommandHandler(IEnumerable<Assembly> assemblies)
        {
            var map = _finder.FindTypesOf<ICommandHandler>(assemblies)
                             .SelectMany(c => GetMethodParameters<ICommand>(c).Select(d => new { Command = d, Handler = new HandlerType(c.AsType(), new ReadOnlyDictionary<Type, Action<object, object>>(CreateHandlerDelegates<ICommand>(c.AsType()))) }))
                             .ToDictionary(c => c.Command, c => c.Handler);
            return new ReadOnlyDictionary<Type, HandlerType>(map);
        }

        public ReadOnlyDictionary<Type, ReadOnlyDictionary<Type, Action<object,object>>> CreateFromAggregate(IEnumerable<Assembly> assemblies)
        {
            var map = _finder.FindTypesOf<Aggregate>(assemblies)
                             .Select(c => new { Aggregate = c, Handler = new ReadOnlyDictionary<Type, Action<object, object>>(CreateHandlerDelegates<IEvent>(c.AsType())) })
                             .ToDictionary(c=>c.Aggregate.AsType(), c=>c.Handler);
            return new ReadOnlyDictionary<Type, ReadOnlyDictionary<Type, Action<object, object>>>(map);
        }

        private IEnumerable<Type> GetMethodParameters<T>(TypeInfo commandHandler)
        {
            return commandHandler.DeclaredMethods
                                 .Where(DoesMethodHandle<T>)
                                 .Select(c => c.GetParameters().First().ParameterType.GetTypeInfo().AsType());
        }

        private Dictionary<Type, Action<object, object>> CreateHandlerDelegates<T>(Type commandHandlerType)
        {
            return commandHandlerType.GetTypeInfo()
                                     .DeclaredMethods.Where(DoesMethodHandle<T>)
                                     .Select(c => new { Command = c.GetParameters().First().ParameterType, HandleMethod = CreateHandleDelegate(commandHandlerType, c) })
                                     .ToDictionary(c => c.Command, c => c.HandleMethod);
        }

        private Action<object, object> CreateHandleDelegate(Type commandHandlerType, MethodInfo handle)
        {
            var parameterType = handle.GetParameters().First().ParameterType;

            var handler = Expression.Parameter(typeof(object), "handler");
            var argument = Expression.Parameter(typeof(object), "parameter");

            var methodCall = Expression.Call(Expression.Convert(handler, commandHandlerType),
                                             handle,
                                             Expression.Convert(argument, parameterType));

            return Expression.Lambda<Action<object, object>>(methodCall, handler, argument).Compile();
        }

        private bool DoesMethodHandle<T>(MethodInfo methodInfo)
        {
            return methodInfo.IsPublic &&
                   methodInfo.Name == "Handle" &&
                   methodInfo.GetParameters().Length == 1 &&
                   methodInfo.GetParameters().First().ParameterType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(T));
        }

    }

    internal class HandlerType
    {
        public Type Handler { get; private set; }
        public ReadOnlyDictionary<Type, Action<object, object>> HandleMethods { get; private set; }

        public HandlerType(Type handler,
                           ReadOnlyDictionary<Type, Action<object, object>> handleMethods)
        {
            Handler = handler;
            HandleMethods = handleMethods;
        }
    }
}
