﻿using RabbitMQ.Client;

namespace Huawei.RabbitMqSubscriberService.Services
{
    public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public static string QueueName = "MailValidationQueue";
        public static string ExchangeName = "MailValidationDirectExchange";
        public static string RoutingKey = "MailValidation";

        private readonly ILogger<RabbitMQClientService> _logger;

        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect()
        {
            try
            {
                _logger.LogInformation($"Connect metodu factory uri bilgisi: {_connectionFactory.Uri.ToString()}");
                _connection = _connectionFactory.CreateConnection();
                _logger.LogInformation($"Connect metodu connection endpoint bilgisi: {_connection.Endpoint.ToString()}");
                if (_channel is { IsOpen: true })
            {
                return _channel;
            }
                _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(ExchangeName, type: "direct", true, false);
                _channel.QueueDeclare(QueueName, true, false, false, null);
                _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingKey);

                _logger.LogInformation("RabbitMQ ile bağlantı kuruldu...");
                return _channel;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Failed to connect {_connection.Endpoint.HostName}");
                _logger.LogInformation($"Failed to connect {_connection.Endpoint.ToString()}");
                _logger.LogError("Gerçekleşen Hata:{0}",ex.ToString());
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ ile bağlantı koptu...");
        }
    }
}
