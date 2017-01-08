using BaseTypes;
using System;
using System.IO;

namespace CommandProcessor
{
    public class FileSnapshotRepository : ISnapshotRepository
    {
        private readonly FileSnapshotRepositoryConfiguration _configuration;

        public FileSnapshotRepository(FileSnapshotRepositoryConfiguration configuration)
        {
            _configuration = configuration;
        }

        public PersitableSnapshot Load(string filename)
        {
            if (File.Exists(filename))
            {
                byte[] bytes = File.ReadAllBytes(filename);
                //TODO
                //Deserialize int a SnapshotResult
            }

            return new PersitableSnapshot(0, new byte[0]);
        }

        public bool Save(string filename, PersitableSnapshot persistableSnapshot)
        {
            var fullPath = Path.Combine(_configuration.Path, filename);
            try
            {
                //TODO
                //Serialize persistableSnapshot into byte[]
                File.WriteAllBytes(fullPath, new byte[0]);
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
