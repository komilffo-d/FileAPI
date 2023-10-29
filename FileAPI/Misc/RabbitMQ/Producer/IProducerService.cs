namespace FileAPI.Misc.RabbitMQ.Producer
{
    public interface IProducerService
    {
        void SendMessage(string message);
    }
}
