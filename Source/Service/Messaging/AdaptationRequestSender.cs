using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Service.Configuration;
using Service.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Service.Messaging
{
    public class AdaptationRequestSender : IAdaptationRequestSender
    {
        private bool disposedValue;

        private readonly IResponseProcessor _responseProcessor;
        private readonly ILogger<AdaptationRequestSender> _logger;

        private readonly BlockingCollection<AdaptationOutcome> _respQueue = new BlockingCollection<AdaptationOutcome>();

        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly EventingBasicConsumer _consumer;
        
        public AdaptationRequestSender(IResponseProcessor responseProcessor, ILogger<AdaptationRequestSender> logger, IArchiveProcessorConfig config)
        {
            _responseProcessor = responseProcessor ?? throw new ArgumentNullException(nameof(responseProcessor));
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
                    _logger.LogInformation($"Received message: Exchange Name: '{ea.Exchange}', Routing Key: '{ea.RoutingKey}'");
                    var headers = ea.BasicProperties.Headers;
                    var body = ea.Body.ToArray();

                    var response = _responseProcessor.Process(headers, body);
                    //_channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                    _respQueue.Add(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error Processing 'input'");
                    _respQueue.Add(AdaptationOutcome.Error);
                }
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

        public AdaptationOutcome Send(string fileId, string originalStoreFilePath, string rebuiltStoreFilePath, CancellationToken processingCancellationToken)
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

            return _respQueue.Take(processingCancellationToken);
        }
    }
}
