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

        public byte[] Load(string filename)
        {
            return File.ReadAllBytes(filename);
        }

        public bool Save(string filename, byte[] snapshot)
        {
            var fullPath = Path.Combine(_configuration.Path, filename);
            try
            {                
                File.WriteAllBytes(fullPath, snapshot);
            }
            catch(Exception e)
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
