using System.IO;

namespace Service
{
    public class LocalFileManager : IFileManager
    {
        public void CopyFile(string sourcePath, string outputPath)
        {
            File.Copy(sourcePath, outputPath);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public byte[] ReadFile(string path)
        {
            return File.ReadAllBytes(path);
        }

        public void WriteFile(string path, byte[] data)
        {
            File.WriteAllBytes(path, data);
        }
    }
}
