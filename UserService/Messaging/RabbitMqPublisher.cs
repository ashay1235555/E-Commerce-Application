using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Net.Security;

namespace UserService.Messaging
{
    public class RabbitMqPublisher
    {
        private readonly IConfiguration _config;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqPublisher(IConfiguration config)
        {
            _config = config;

            var factory = new ConnectionFactory
            {
                Uri = new Uri(_config["RabbitMQ:ConnectionUrl"]),
                Ssl = new SslOption
                {
                    Enabled = true,
                    Version = System.Security.Authentication.SslProtocols.Tls12,

                    // FIX: CloudAMQP uses wildcard certs 
                    ServerName = "",

                    // FIX: Accept Certificate Name Mismatch
                    AcceptablePolicyErrors =
                        SslPolicyErrors.RemoteCertificateNameMismatch |
                        SslPolicyErrors.RemoteCertificateChainErrors
                }
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: _config["RabbitMQ:Exchange"],
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false
            );
        }

        public void PublishEvent(string routingKey, object messageObj)
        {
            string message = JsonSerializer.Serialize(messageObj);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: _config["RabbitMQ:Exchange"],
                routingKey: routingKey,
                basicProperties: null,
                body: body
            );
        }
    }
}
