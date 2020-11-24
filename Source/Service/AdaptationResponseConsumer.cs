using Microsoft.Extensions.Logging;
using Service.Configuration;
using Service.Enums;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    public class AdaptationResponseConsumer : IAdaptationResponseConsumer
    {
        private readonly IAdaptationResponseCollection _collection;
        private readonly IArchiveManager _archiveManager;
        private readonly IArchiveProcessorConfig _config;
        private readonly ILogger<AdaptationResponseConsumer> _logger;

        public AdaptationResponseConsumer(IAdaptationResponseCollection collection, IArchiveManager archiveManager, IArchiveProcessorConfig config, ILogger<AdaptationResponseConsumer> logger)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _archiveManager = archiveManager ?? throw new ArgumentNullException(nameof(archiveManager));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task ConsumeResponses(IDictionary<string, string> fileMappings, string rebuiltDir, string originalDir, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                while (!_collection.IsCompleted)
                {
                    var response = _collection.Take(token);

                    _logger.LogInformation($"Archive File Id: {_config.ArchiveFileId}, Archived File Id: {response.Key}, status: {response.Value}");

                    if (response.Value == AdaptationOutcome.Replace)
                    {
                        _archiveManager.AddToArchive(_config.OutputPath, $"{rebuiltDir}/{response.Key}", fileMappings[response.Key.ToString()]);
                    }
                    else if (response.Value == AdaptationOutcome.Unmodified)
                    {
                        _archiveManager.AddToArchive(_config.OutputPath, $"{originalDir}/{response.Key}", fileMappings[response.Key.ToString()]);
                    }
                }
            });
        }
    }
}
