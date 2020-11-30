using Microsoft.Extensions.Logging;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace Service
{
    public class ZipArchiveManager : IArchiveManager
    {
        private readonly ILogger<ZipArchiveManager> _logger;

        public ZipArchiveManager(ILogger<ZipArchiveManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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

        public IDictionary<Guid, string> ExtractArchive(string archiveFilePath, string targetPath)
        {
            try
            {
                var fileMapping = new Dictionary<Guid, string>();

                using (var archive = ZipFile.OpenRead(archiveFilePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        // Entry is a folder within the archive, don't create mapping and extract.
                        if (entry.FullName.EndsWith("/"))
                            continue;

                        var fileId = Guid.NewGuid();
                        fileMapping.Add(fileId, entry.FullName);
                        entry.ExtractToFile($"{targetPath}/{fileId}");
                    }
                }

                return fileMapping;
            }
            catch(Exception e)
            {
                _logger.LogError($"Archive File Path: {archiveFilePath}, error extracting archive. {e.Message}");
                return null;
            }
        }
    }
}
