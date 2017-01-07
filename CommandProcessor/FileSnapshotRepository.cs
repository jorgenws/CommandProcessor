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

        public Tuple<byte[], int> Load(string filename)
        {
            if (File.Exists(filename))
            {
                byte[] bytes = File.ReadAllBytes(filename);
                //TODO
                //Deserialize int a Tuple<byte[],int>
                //Should probably make an actual type for it...
                //Item1 and item2 looks stupid
            }

            return new Tuple<byte[], int>(new byte[0], 0);
        }

        public bool Save(string filename, byte[] snapshot)
        {
            var fullPath = Path.Combine(_configuration.Path, filename);
            try
            {                
                File.WriteAllBytes(fullPath, snapshot);
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
