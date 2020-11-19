using Service.Configuration;
using Service.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Service
{
    public class ArchiveProcessor : IArchiveProcessor
    {
        private readonly IAdaptationOutcomeSender _adaptationOutcomeSender;
        private readonly IFileManager _fileManager;
        private readonly IArchiveManager _archiveManager;
        private readonly IArchiveProcessorConfig _config;
        private readonly TimeSpan _processingTimeoutDuration;

        private string _tmpOriginalDirectory => $"{_config.InputPath}_tmp";

        public ArchiveProcessor(IAdaptationOutcomeSender adaptationOutcomeSender, IFileManager fileManager, IArchiveManager archiveManager,  IArchiveProcessorConfig config)
        {
            _adaptationOutcomeSender = adaptationOutcomeSender ?? throw new ArgumentNullException(nameof(adaptationOutcomeSender));
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            _archiveManager = archiveManager ?? throw new ArgumentNullException(nameof(archiveManager));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _processingTimeoutDuration = _config.ProcessingTimeoutDuration;
        }

        public void Process()
        {
            var task = Task.Run(() =>
            {
                return ProcessArchive();
            });

            try
            {
                bool isCompletedSuccessfully = task.Wait(_processingTimeoutDuration);

                if (!isCompletedSuccessfully)
                {
                    Console.WriteLine($"Error: File Id: {_config.ArchiveFileId} exceeded {_processingTimeoutDuration}s");
                    ClearRebuiltStore(_config.OutputPath);
                    ClearSourceStore(_tmpOriginalDirectory);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: File Id: {_config.ArchiveFileId} threw exception {e.Message}");
                ClearRebuiltStore(_config.OutputPath);
                ClearSourceStore(_tmpOriginalDirectory);
                _adaptationOutcomeSender.Send(FileOutcome.Error, _config.ArchiveFileId, _config.ReplyTo);
            }
        }

        private Task ProcessArchive()
        {
            Console.WriteLine($"File Id: {_config.ArchiveFileId} processing requested");
            
            if (!_fileManager.FileExists(_config.InputPath))
            {
                throw new FileNotFoundException($"File Id: {_config.ArchiveFileId} does not exist at {_config.InputPath}");
            }

            _fileManager.CreateDirectory(_tmpOriginalDirectory);

            Console.WriteLine($"File Id: {_config.ArchiveFileId} Extracting archive to temp folder {_tmpOriginalDirectory}");
            _archiveManager.ExtractArchive(_config.InputPath, _tmpOriginalDirectory);

            Console.WriteLine($"File Id: {_config.ArchiveFileId} Creating archive.");
            _archiveManager.CreateArchive(_tmpOriginalDirectory, _config.OutputPath);

            _adaptationOutcomeSender.Send(FileOutcome.Replace, _config.ArchiveFileId, _config.ReplyTo);
            ClearSourceStore(_tmpOriginalDirectory);

            return Task.CompletedTask;
        }

        private void ClearRebuiltStore(string path)
        {
            if (_fileManager.FileExists(path))
            {
                _fileManager.DeleteFile(path);
            }
        }

        private void ClearSourceStore(string tempDirectory)
        {
            if (_fileManager.DirectoryExists(tempDirectory))
            {
                _fileManager.DeleteDirectory(tempDirectory);
            }
        }
    }
}
