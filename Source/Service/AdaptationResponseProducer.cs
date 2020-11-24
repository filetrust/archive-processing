using Microsoft.Extensions.Logging;
using Service.Configuration;
using Service.Interfaces;
using Service.Messaging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    public class AdaptationResponseProducer : IAdaptationResponseProducer
    {
        private readonly IFileManager _fileManager;
        private readonly IAdaptationRequestSender _adaptationRequestSender;
        private readonly IArchiveProcessorConfig _config;
        private readonly ILogger<AdaptationResponseProducer> _logger;

        public AdaptationResponseProducer(IFileManager fileManager, IAdaptationRequestSender adaptationRequestSender, IArchiveProcessorConfig config, ILogger<AdaptationResponseProducer> logger)
        {
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            _adaptationRequestSender = adaptationRequestSender ?? throw new ArgumentNullException(nameof(adaptationRequestSender));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task SendMessages(string originalDirectory, string rebuiltDirectory, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                Parallel.ForEach(_fileManager.GetFiles(originalDirectory), (originalFilePath) => {
                    var archivedFileId = Path.GetFileName(originalFilePath);
                    var rebuiltPath = $"{rebuiltDirectory}/{archivedFileId}";

                    _logger.LogInformation($"Archive File Id: {_config.ArchiveFileId}, Archived File Id: {archivedFileId} about to be sent");

                    _adaptationRequestSender.Send(archivedFileId, originalFilePath, rebuiltPath, token);
                });
            });
        }
    }
}
