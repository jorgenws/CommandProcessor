using System;

namespace BaseTypes
{
    public interface ISnapshotRepository
    {
        Tuple<byte[],int> Load(string filename);
        bool Save(string filename, byte[] snapshot);
    }
}
