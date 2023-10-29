using Microsoft.Extensions.Options;
using RabbitMQ.Client;
namespace FileAPI.Misc.RabbitMQ
{
    public sealed class RabbitMQService : IRabbitMQService
    {
        private readonly RabbitMQConfiguration _configuration;

        private IConnection _connection;

        public RabbitMQService(IOptions<RabbitMQConfiguration> options)
        {
            _configuration = options.Value;
        }
        public IConnection CreateChannel()
        {
            if (_connection is null)
            {
                ConnectionFactory connection = new ConnectionFactory()
                {
                    UserName = _configuration.Username,
                    Password = _configuration.Password,
                    HostName = _configuration.HostName
                };
                connection.DispatchConsumersAsync = true;
                _connection = connection.CreateConnection();
                return _connection;
            }
            return _connection;
        }

        /* protected override IModel GetRabbitMQChannel(string queueName)
         {
             if (_connection is not null && _connection.IsOpen)
             {
                 IModel model = _connection.CreateModel();
                 model.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                 var args = new Dictionary<string, object>();
                 args.Add("x-message-ttl", 60000);
                 model.QueueDeclare(queueName, false, true, false, args);
                 model.QueueBind(queueName, exchangeName, routingKey, null);
                 return model;
             }
             return null;
         }



         public override void SendMessage(string message, string queueName)
         {
             if (_connection is not null && _connection.IsOpen)
             {
                 IModel model = GetRabbitMQChannel(queueName);
                 byte[] messageBodyBytes = Encoding.UTF8.GetBytes(message);
                 model.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);
             }
         }

         public override string ReceiveMessage(string queueName)
         {
             if (_connection is null)
                 CreateConnection();
             if (_connection is not null &&_connection.IsOpen)
             {
                 string originalMessage = String.Empty;
                 IModel model = GetRabbitMQChannel(queueName);
                 BasicGetResult result = model.BasicGet(queueName, false);
                 if (result is not null)
                 {
                     byte[] body = result.Body.ToArray();
                     originalMessage = Encoding.UTF8.GetString(body);
                 }
                 return originalMessage;
             }
             return null;

         }

         public override void Close()
         {
             if (_model.IsOpen)
                 _model.Close();
             if (_connection.IsOpen)
                 _connection.Close();
         }*/
    }
}
