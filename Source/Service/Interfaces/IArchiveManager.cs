using System;
using System.Collections.Generic;

namespace Service.Interfaces
{
    public interface IArchiveManager
    {
        void AddToArchive(string archiveFilePath, string sourceFilePath, string fileName);
        void CreateArchive(string sourceFolderPath, string archiveFilePath);
        IDictionary<Guid, string> ExtractArchive(string archiveFilePath, string targetPath);
    }
}
