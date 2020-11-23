using System;
using System.Collections.Generic;

namespace Service
{
    public interface IArchiveManager
    {
        void AddToArchive(string archiveFilePath, string sourceFilePath, string fileName);
        void CreateArchive(string sourceFolderPath, string archiveFilePath);
        Dictionary<string, string> ExtractArchive(string archiveFilePath, string targetPath);
    }
}
