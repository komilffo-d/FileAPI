using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace FileAPI.Misc.RabbitMQ.Consumer
{
    public class ConsumerService
    {
/*        private IConnection _connection { get; set; }
        private IModel _model { get; set; }

        private const string exchangeName = "file";
        private const string routingKey = "file.key";
        public ConsumerService(IRabbitMqService rabbitMqService)
        {
            _connection = rabbitMqService.CreateChannel();
            _model = _connection.CreateModel();
            _model.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
            _model.ExchangeDeclare("your.exchange.name", ExchangeType.Fanout, durable: true, autoDelete: false);
            _model.QueueBind(_queueName, "your.exchange.name", string.Empty);
        }

        public async Task ReadMessgaes()
        {
            var consumer = new AsyncEventingBasicConsumer(_model);
            consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var text = System.Text.Encoding.UTF8.GetString(body);
                Console.WriteLine(text);
                await Task.CompletedTask;
                _model.BasicAck(ea.DeliveryTag, false);
            };
            _model.BasicConsume(_queueName, false, consumer);
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_model.IsOpen)
                _model.Close();
            if (_connection.IsOpen)
                _connection.Close();
        }*/
    }
}
