using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace Service
{
    public class ZipArchiveManager : IArchiveManager
    {
        public void AddToArchive(string archiveFilePath, string sourceFilePath, string fileName)
        {
            using (var archive = ZipFile.Open(archiveFilePath, ZipArchiveMode.Update))
            {
                archive.CreateEntryFromFile(sourceFilePath, fileName);
            }
        }

        public void CreateArchive(string sourceFolderPath, string archiveFilePath)
        {
            ZipFile.CreateFromDirectory(sourceFolderPath, archiveFilePath);
        }

        public IDictionary<string, string> ExtractArchive(string archiveFilePath, string targetPath)
        {
            var fileMapping = new Dictionary<string, string>();

            using (var archive = ZipFile.OpenRead(archiveFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    // Entry is a folder within the archive, don't create mapping and extract.
                    if (entry.FullName.EndsWith("/"))
                        continue;

                    var fileId = Guid.NewGuid().ToString();
                    fileMapping.Add(fileId, entry.FullName);
                    entry.ExtractToFile($"{targetPath}/{fileId}");
                }
            }

            return fileMapping;
        }
    }
}
