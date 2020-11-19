namespace Service
{
    public interface IFileManager
    {
        void CopyFile(string sourcePath, string outputPath);
        void CreateDirectory(string path);
        void DeleteDirectory(string path);
        void DeleteFile(string path);
        bool DirectoryExists(string path);
        bool FileExists(string path);
        byte[] ReadFile(string path);
        void WriteFile(string path, byte[] data);
    }
}
