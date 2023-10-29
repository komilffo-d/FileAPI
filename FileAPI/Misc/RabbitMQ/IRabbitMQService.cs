using RabbitMQ.Client;

namespace FileAPI.Misc.RabbitMQ
{
    public interface IRabbitMQService
    {
        IConnection CreateChannel();
    }
}
