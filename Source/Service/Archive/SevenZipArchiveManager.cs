using Microsoft.Extensions.Logging;
using Service.Exceptions;
using Service.Interfaces;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace Service.Archive
{
    public class SevenZipArchiveManager : IArchiveManager
    {
        private readonly ILogger<SevenZipArchiveManager> _logger;

        public SevenZipArchiveManager(ILogger<SevenZipArchiveManager> logger)
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

                using (var archive = SevenZipArchive.Open(archiveFilePath))
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        var fileId = Guid.NewGuid();
                        fileMapping.Add(fileId, entry.Key);
                        entry.WriteToFile($"{targetPath}/{fileId}");
                    }
                }

                return fileMapping;
            }
            catch (Exception e)
            {
                _logger.LogError($"Archive File Path: {archiveFilePath}, error extracting archive. {e.Message}");
                throw new FileEncryptedException(e.Message);
            }
        }
    }
}
