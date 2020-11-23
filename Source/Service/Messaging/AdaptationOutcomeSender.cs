using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Service.Configuration;
using System;
using System.Collections.Generic;

namespace Service.Messaging
{
    public class AdaptationOutcomeSender : IAdaptationOutcomeSender, IDisposable
    {
        private bool disposedValue;

        private readonly ILogger<AdaptationOutcomeSender> _logger;
        private readonly IModel _channel;
        private readonly IConnection _connection;

        public AdaptationOutcomeSender(ILogger<AdaptationOutcomeSender> logger, IArchiveProcessorConfig config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (config == null) throw new ArgumentNullException(nameof(config));

            var connectionFactory = new ConnectionFactory() { 
                HostName = config.AdaptationRequestQueueHostname,
                Port = config.AdaptationRequestQueuePort,
                UserName = config.MessageBrokerUser,
                Password = config.MessageBrokerPassword
            };

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            _logger.LogInformation($"AdaptationOutcomeSender Connection established to {config.AdaptationRequestQueueHostname}");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _channel?.Dispose();
                    _connection?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Send(string status, string fileId, string replyTo)
        {
            var headers = new Dictionary<string, object>()
                {
                    { "file-id", fileId },
                    { "file-outcome", status },
                };

            var replyProps = _channel.CreateBasicProperties();
            replyProps.Headers = headers;
            _channel.BasicPublish("", replyTo, basicProperties: replyProps);

            _logger.LogInformation($"Sent Message, ReplyTo: {replyTo}, FileId: {fileId}, Outcome: {status}");
        }
    }
}
