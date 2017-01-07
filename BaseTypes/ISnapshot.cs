using System;

namespace BaseTypes
{
    public interface ISnapshot
    {
        Guid Id { get; }
        void LoadSnapshot(byte[] bytes);
        byte[] SaveSnapshot();
        int HighestEventNumber { get; }
    }

    public interface ISnapshotRepository
    {
        byte[] Load(string filename);
        bool Save(string filename, byte[] snapshot);
    }
}
