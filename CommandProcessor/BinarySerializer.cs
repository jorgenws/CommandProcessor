using BaseTypes;
using ProtoBuf;
using ProtoBuf.Meta;
using System.IO;

namespace CommandProcessor
{
    internal class BinarySnapshotSerializer : ProtobufSerializer, IBinarySnapshotSerializer
    {
        public BinarySnapshotSerializer()
        {
            if (!RuntimeTypeModel.Default.IsDefined(typeof(PersitableSnapshot)))
                RuntimeTypeModel.Default.Add(typeof(PersitableSnapshot), false)
                                        .Add(1, "SnapshotFromId")
                                        .Add(2, "Snapshot");
        }

        public PersitableSnapshot Deserialize(byte[] item)
        {
            return Deserialize<PersitableSnapshot>(item);
        }

        public byte[] Serialize(PersitableSnapshot item)
        {
            return Serialize(item);
        }
    }

    internal class ProtobufSerializer
    {
        internal T Deserialize<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }

        internal byte[] Serialize<T>(T item)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, item);
                return ms.ToArray();
            }
        }
    }
}