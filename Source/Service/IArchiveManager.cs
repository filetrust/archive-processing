namespace Service
{
    public interface IArchiveManager
    {
        void CreateArchive(string sourceFolderPath, string archiveFilePath);
        void ExtractArchive(string archiveFilePath, string targetPath);
    }
}
