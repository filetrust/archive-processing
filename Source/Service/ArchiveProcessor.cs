using Service.Configuration;
using Service.Messaging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Service
{
    public class ArchiveProcessor : IArchiveProcessor
    {
        private readonly IAdaptationOutcomeSender _adaptationOutcomeSender;
        private readonly IFileManager _fileManager;
        private readonly IArchiveProcessorConfig _config;
        private readonly TimeSpan _processingTimeoutDuration;

        public ArchiveProcessor(IAdaptationOutcomeSender adaptationOutcomeSender, IFileManager fileManager, IArchiveProcessorConfig config)
        {
            _adaptationOutcomeSender = adaptationOutcomeSender ?? throw new ArgumentNullException(nameof(adaptationOutcomeSender));
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
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
                    Console.WriteLine($"Error: Processing 'input' {_config.ArchiveFileId} exceeded {_processingTimeoutDuration}s");
                    ClearRebuiltStore(_config.OutputPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: Processing 'input' {_config.ArchiveFileId} threw exception {e.Message}");
                ClearRebuiltStore(_config.OutputPath);
                _adaptationOutcomeSender.Send(FileOutcome.Error, _config.ArchiveFileId, _config.ReplyTo);
            }
        }

        private Task ProcessArchive()
        {
            Console.WriteLine($"File Id: {_config.ArchiveFileId} processing requested");
            
            if (!_fileManager.FileExists(_config.InputPath))
            {
                throw new FileNotFoundException($"File does not exist at {_config.InputPath}");
            }

            _fileManager.CopyFile(_config.InputPath, _config.OutputPath);

            _adaptationOutcomeSender.Send(FileOutcome.Replace, _config.ArchiveFileId, _config.ReplyTo);

            return Task.CompletedTask;
        }

        private void ClearRebuiltStore(string path)
        {
            if (_fileManager.FileExists(path))
            {
                _fileManager.DeleteFile(path);
            }
        }
    }
}
