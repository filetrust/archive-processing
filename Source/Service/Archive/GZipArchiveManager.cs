using Microsoft.Extensions.Logging;
using Service.Exceptions;
using Service.Interfaces;
using SharpCompress.Archives;
using SharpCompress.Archives.GZip;
using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.Archive
{
    public class GZipArchiveManager : IArchiveManager
    {
        private readonly ILogger<GZipArchiveManager> _logger;

        public GZipArchiveManager(ILogger<GZipArchiveManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddToArchive(string archiveFilePath, string sourceFilePath, string fileName)
        {
            using (var archive = GZipArchive.Create())
            {
                archive.AddEntry(fileName, sourceFilePath);
                archive.SaveTo(archiveFilePath, new WriterOptions(CompressionType.GZip));
            }
        }

        public void CreateArchive(string sourceFolderPath, string archiveFilePath)
        {
            // Not needed for GZip, GZip can only compress a single file
        }

        public IDictionary<Guid, string> ExtractArchive(string archiveFilePath, string targetPath)
        {
            try
            {
                var fileMapping = new Dictionary<Guid, string>();

                using (var archive = GZipArchive.Open(archiveFilePath))
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
            catch(Exception e)
            {
                _logger.LogError($"Archive File Path: {archiveFilePath}, error extracting archive. {e.Message}");
                throw new FileEncryptedException(e.Message);
            }
        }
    }
}
