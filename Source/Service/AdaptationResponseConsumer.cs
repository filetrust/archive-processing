using Microsoft.Extensions.Logging;
using Service.Configuration;
using Service.Enums;
using Service.ErrorReport;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    public class AdaptationResponseConsumer : IAdaptationResponseConsumer
    {
        private const string ErrorReportFileName = "ErrorReport.html";

        private readonly IAdaptationResponseCollection _collection;
        private readonly IArchiveManager _archiveManager;
        private readonly IErrorReportGenerator _errorReportGenerator;
        private readonly IFileManager _fileManager;
        private readonly IArchiveProcessorConfig _config;
        private readonly ILogger<AdaptationResponseConsumer> _logger;

        public AdaptationResponseConsumer(IAdaptationResponseCollection collection, IArchiveManager archiveManager, IErrorReportGenerator errorReportGenerator, IFileManager fileManager, IArchiveProcessorConfig config, ILogger<AdaptationResponseConsumer> logger)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _archiveManager = archiveManager ?? throw new ArgumentNullException(nameof(archiveManager));
            _errorReportGenerator = errorReportGenerator ?? throw new ArgumentNullException(nameof(errorReportGenerator));
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task ConsumeResponses(IDictionary<string, string> fileMappings, string rebuiltDir, string originalDir, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                string errorReport = null;

                while (!_collection.IsCompleted)
                {
                    if (_collection.TryTake(out var response, token))
                    {
                        _logger.LogInformation($"Archive File Id: {_config.ArchiveFileId}, Archived File Id: {response.Key}, status: {response.Value}");

                        if (response.Value == AdaptationOutcome.Replace)
                        {
                            _archiveManager.AddToArchive(_config.OutputPath, $"{rebuiltDir}/{response.Key}", fileMappings[response.Key.ToString()]);
                        }
                        else if (response.Value == AdaptationOutcome.Unmodified)
                        {
                            _archiveManager.AddToArchive(_config.OutputPath, $"{originalDir}/{response.Key}", fileMappings[response.Key.ToString()]);
                        }
                        else
                        {
                            errorReport = _errorReportGenerator.AddIdToReport($"{_config.ArchiveFileId}/{response.Key}");
                        }
                    }
                }

                if (errorReport != null)
                {
                    _fileManager.WriteFile($"{rebuiltDir}/{ErrorReportFileName}", Encoding.UTF8.GetBytes(errorReport));
                    _archiveManager.AddToArchive(_config.OutputPath, $"{rebuiltDir}/{ErrorReportFileName}", ErrorReportFileName);
                }
            });
        }
    }
}
