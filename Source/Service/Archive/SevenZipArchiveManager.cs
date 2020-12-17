using Microsoft.Extensions.Logging;
using Service.Exceptions;
using Service.Interfaces;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.IO;
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
            var tempArchiveName = $"{archiveFilePath}_archiveTmp";

            using (var archive = SharpCompress.Archives.Zip.ZipArchive.Open(archiveFilePath))
            {
                archive.AddEntry(fileName, sourceFilePath);
                archive.SaveTo(tempArchiveName, new WriterOptions(CompressionType.LZMA));
            }

            File.Copy(tempArchiveName, archiveFilePath, true);
            File.Delete(tempArchiveName);
        }

        public void CreateArchive(string sourceFolderPath, string archiveFilePath)
        {
            using (var archive = SharpCompress.Archives.Zip.ZipArchive.Create())
            {
                archive.SaveTo(archiveFilePath, new WriterOptions(CompressionType.LZMA));
            }
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
