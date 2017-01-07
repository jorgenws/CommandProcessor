using BaseTypes;
using System;

namespace CommandProcessor
{
    internal class SnapshotFileNameBuilder : ISnapshotFileNameBuilder
    {
        public string Build(Type aggregateType, Guid aggregateId)
        {
            return $"{aggregateId.ToString("D")}-{aggregateType}";
        }
    }
}