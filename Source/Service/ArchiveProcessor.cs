using Service.Configuration;
using Service.Messaging;
using System;
using System.IO;

namespace Service
{
    public class ArchiveProcessor : IArchiveProcessor
    {
        private readonly IAdaptationOutcomeSender _adaptationOutcomeSender;
        private readonly IArchiveProcessorConfig _config;

        public ArchiveProcessor(IAdaptationOutcomeSender adaptationOutcomeSender, IArchiveProcessorConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _adaptationOutcomeSender = adaptationOutcomeSender ?? throw new ArgumentNullException(nameof(adaptationOutcomeSender));
        }

        public void Process()
        {
            Console.WriteLine($"File Id: {_config.ArchiveFileId} processing requested");
            File.Copy(_config.InputPath, _config.OutputPath);

            _adaptationOutcomeSender.Send("replace", _config.ArchiveFileId, _config.ReplyTo);
        }
    }
}
