using RabbitMQ.Client;
using System.Text;

namespace FileAPI.Misc.RabbitMQ.Producer
{
    public class ProducerService : IProducerService
    {
        private readonly IModel _model;
        private readonly IConnection _connection;
        private const string queueName = "file";
        private const string exchangeName = "file";
        private const string routingKey = "file.key";
        public void SendMessage(string message)
        {
            if (_connection is not null && _connection.IsOpen)
            {
                IModel model = GetRabbitMQChannel();
                byte[] messageBodyBytes = Encoding.UTF8.GetBytes(message);
                model.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);
            }
        }
        protected IModel GetRabbitMQChannel()
        {
            if (_connection is not null && _connection.IsOpen && _model is null)
            {
                IModel model = _connection.CreateModel();
                model.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                var args = new Dictionary<string, object>();
                args.Add("x-message-ttl", 60000);
                model.QueueDeclare(queueName, false, true, false, args);
                model.QueueBind(queueName, exchangeName, routingKey, null);
                return model;
            }
            return _model;
        }
    }
}
