namespace BaseTypes
{
    public interface ISnapshotRepository
    {
        PersitableSnapshot Load(string filename);
        bool Save(string filename, PersitableSnapshot snapshot);
    }

    public class PersitableSnapshot
    {
        public int SnapshotFromId { get; private set; }
        public byte[] Snapshot { get; private set; }

        public PersitableSnapshot(int snapshotFromId, byte[] snapshot)
        {
            SnapshotFromId = snapshotFromId;
            Snapshot = snapshot;
        }
    }
}
