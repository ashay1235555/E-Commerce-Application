//using Microsoft.EntityFrameworkCore.Metadata;
//using Microsoft.Extensions.Configuration;
//using RabbitMQ.Client;
//using System;
//using System.Text;
//using System.Text.Json;

//namespace UserService.Messaging
//{
//    public class RabbitMqPublisher
//    {
//        private readonly IConfiguration _config;
//        private readonly IConnection _connection;
//        private readonly RabbitMQ.Client.IModel _channel;

//        public RabbitMqPublisher(IConfiguration config)
//        {
//            _config = config;

//            var factory = new ConnectionFactory()
//            {
//                Uri = new Uri(_config["RabbitMQ:ConnectionUrl"]),
//                Ssl = new SslOption { Enabled = true }
//            };

//            _connection = factory.CreateConnection();  // <-- Method appears only if type matches
//            _channel = _connection.CreateModel();      // <-- Should now appear

//            _channel.ExchangeDeclare(
//                exchange: _config["RabbitMQ:Exchange"],
//                type: ExchangeType.Direct,
//                durable: true,
//                autoDelete: false
//            );
//        }

//        public void PublishEvent(object message)
//        {
//            string jsonString = JsonSerializer.Serialize(message);
//            byte[] body = Encoding.UTF8.GetBytes(jsonString);

//            _channel.BasicPublish(
//                exchange: _config["RabbitMQ:Exchange"],
//                routingKey: "",
//                basicProperties: null,
//                body: body
//            );
//        }
//    }
//}
