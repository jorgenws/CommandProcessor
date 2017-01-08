namespace BaseTypes
{
    public interface IBinarySnapshotSerializer
    {
        byte[] Serialize(PersitableSnapshot item);
        PersitableSnapshot Deserialize(byte[] item);
    }
}
