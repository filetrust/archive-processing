using System;
using System.IO.Compression;

namespace Service
{
    public class ZipArchiveManager : IArchiveManager
    {
        public void CreateArchive(string sourceFolderPath, string archiveFilePath)
        {
            ZipFile.CreateFromDirectory(sourceFolderPath, archiveFilePath);
        }

        public void ExtractArchive(string archiveFilePath, string targetPath)
        {
            ZipFile.ExtractToDirectory(archiveFilePath, targetPath);
        }
    }
}
