namespace BaseTypes
{
    public interface ISnapshotRepository
    {
        PersitableSnapshot Load(string filename);
        bool Save(string filename, PersitableSnapshot snapshot);
    }

    public class PersitableSnapshot
    {
        public int SnapshotFromId { get; set; }
        public byte[] Snapshot { get; set; }
    }
}
