using System;
using System.Collections.ObjectModel;

namespace BaseTypes
{
    public abstract class Aggregate
    {
        public Guid Id { get; private set; }

        private ReadOnlyDictionary<Type, Action<object, object>> _handleMethods;

        internal void SetUp(Guid id, ReadOnlyDictionary<Type, Action<object, object>> handleMethods)
        {
            Id = id;
            _handleMethods = handleMethods;
        }

        internal void Handle(object @event)
        {
            if (_handleMethods.ContainsKey(@event.GetType()))
                _handleMethods[@event.GetType()](this, @event);
        }
    }
}