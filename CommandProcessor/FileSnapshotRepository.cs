using BaseTypes;
using System;
using System.IO;

namespace CommandProcessor
{
    public class FileSnapshotRepository : ISnapshotRepository
    {
        private readonly FileSnapshotRepositoryConfiguration _configuration;
        private readonly IBinarySnapshotSerializer _binarySnapshotSerializer;

        public FileSnapshotRepository(FileSnapshotRepositoryConfiguration configuration,
                                      IBinarySnapshotSerializer binarySnapshotSerializer)
        {
            _configuration = configuration;
            _binarySnapshotSerializer = binarySnapshotSerializer;
        }

        public PersitableSnapshot Load(string filename)
        {
            if (File.Exists(filename))
            {
                byte[] bytes = File.ReadAllBytes(filename);
                return _binarySnapshotSerializer.Deserialize(bytes);
            }

            return new PersitableSnapshot { SnapshotFromId = -1, Snapshot = new byte[0] };
        }

        public bool Save(string filename, PersitableSnapshot persistableSnapshot)
        {
            var fullPath = Path.Combine(_configuration.Path, filename);
            try
            {
                byte[] serializedSnapshot =_binarySnapshotSerializer.Serialize(persistableSnapshot);
                File.WriteAllBytes(fullPath, serializedSnapshot);
            }
            catch(Exception)
            {
                File.Delete(fullPath);
                return false;
            }

            return true;
        }
    }

    public class FileSnapshotRepositoryConfiguration
    {
        public string Path { get; private set; }

        public FileSnapshotRepositoryConfiguration(string path)
        {
            Path = path;
        }
    }
}
