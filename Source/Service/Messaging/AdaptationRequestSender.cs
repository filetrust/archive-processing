using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Service.Configuration;
using Service.Enums;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Service.Messaging
{
    public class AdaptationRequestSender : IAdaptationRequestSender, IDisposable
    {
        private bool disposedValue;

        private readonly IResponseProcessor _responseProcessor;
        private readonly IAdaptationResponseCollection _collection;
        private readonly ILogger<AdaptationRequestSender> _logger;

        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly EventingBasicConsumer _consumer;

        private int _receivedMessageCount = 0;

        public int ExpectedMessageCount { get; set; }

        public AdaptationRequestSender(IResponseProcessor responseProcessor, IAdaptationResponseCollection collection, ILogger<AdaptationRequestSender> logger, IArchiveProcessorConfig config)
        {
            _responseProcessor = responseProcessor ?? throw new ArgumentNullException(nameof(responseProcessor));
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (config == null) throw new ArgumentNullException(nameof(config));

            var connectionFactory = new ConnectionFactory()
            {
                HostName = config.AdaptationRequestQueueHostname,
                Port = config.AdaptationRequestQueuePort,
                UserName = config.MessageBrokerUser,
                Password = config.MessageBrokerPassword
            };

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _consumer = new EventingBasicConsumer(_channel);

            _channel.BasicConsume(_consumer, "amq.rabbitmq.reply-to", autoAck: true);

            _consumer.Received += (model, ea) =>
            {
                try
                {
                    _receivedMessageCount++;
                    _logger.LogInformation($"Received message: Exchange Name: '{ea.Exchange}', Routing Key: '{ea.RoutingKey}'");
                    var headers = ea.BasicProperties.Headers;
                    var body = ea.Body.ToArray();

                    var response = _responseProcessor.Process(headers);

                    _collection.Add(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error Processing 'input'");
                    _collection.Add(new KeyValuePair<Guid, AdaptationOutcome>(Guid.Empty, AdaptationOutcome.Error));
                }

                if (_receivedMessageCount == ExpectedMessageCount)
                    _collection.CompleteAdding();
            };

            _logger.LogInformation($"AdaptationRequestSender Connection established to {config.AdaptationRequestQueueHostname}");
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

        public void Send(string fileId, string originalStoreFilePath, string rebuiltStoreFilePath, CancellationToken processingCancellationToken)
        {
            IDictionary<string, object> headerMap = new Dictionary<string, object>
            {
                { "file-id", fileId },
                { "request-mode", "respmod" },
                { "source-file-location", originalStoreFilePath},
                { "rebuilt-file-location", rebuiltStoreFilePath}
            };

            string messageBody = JsonConvert.SerializeObject(headerMap, Formatting.None);
            var body = Encoding.UTF8.GetBytes(messageBody);

            var messageProperties = _channel.CreateBasicProperties();
            messageProperties.Headers = headerMap;
            messageProperties.ReplyTo = "amq.rabbitmq.reply-to";

            _logger.LogInformation($"Sending adaptation-request for {fileId}");

            _channel.BasicPublish(exchange: "adaptation-exchange",
                                 routingKey: "adaptation-request",
                                 basicProperties: messageProperties,
                                 body: body);
        }
    }
}
