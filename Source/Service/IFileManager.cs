namespace Service
{
    public interface IFileManager
    {
        void CopyFile(string sourcePath, string outputPath);
        void DeleteFile(string path);
        bool FileExists(string path);
        byte[] ReadFile(string path);
        void WriteFile(string path, byte[] data);
    }
}
