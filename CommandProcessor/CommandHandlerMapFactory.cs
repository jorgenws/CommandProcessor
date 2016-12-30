using BaseTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace CommandProcessor
{
    internal class CommandToHandlerMapFactory
    {
        private TypeFinder _finder;

        public CommandToHandlerMapFactory()
        {
            _finder = new TypeFinder();
        }

        public ReadOnlyDictionary<Type, Type> Create(IEnumerable<Assembly> assemblies)
        {
            var map = _finder.FindTypesOf<ICommandHandler>(assemblies)
                          .SelectMany(c => GetCommandMethodParameters(c).Select(d => new { Handler = c.AsType(), Command = d }))
                          .ToDictionary(c => c.Command, c => c.Handler);

            return new ReadOnlyDictionary<Type, Type>(map);
        }

        private IEnumerable<Type> GetCommandMethodParameters(TypeInfo commandHandler)
        {
            return commandHandler.DeclaredMethods.Where(c => c.IsPublic &&
                                                             c.Name == "Handle" &&
                                                             c.GetParameters().Length == 1 &&
                                                             c.GetParameters().First().ParameterType.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ICommand)))
                                                 .Select(c => c.GetParameters().First().ParameterType.GetTypeInfo().AsType());
        }
    }
}
