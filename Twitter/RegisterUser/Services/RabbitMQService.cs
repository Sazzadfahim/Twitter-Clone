namespace RegisterUser.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        public IConnection CreateChannel()
        {
            ConnectionFactory connection = new ConnectionFactory()
            {
                Uri = new Uri("amqps://gdixhvtv:Pxj12Y0GW0TRGmHNFl9VSKVmeP6IUHeW@beaver.rmq.cloudamqp.com/gdixhvtv")
            };
            connection.DispatchConsumersAsync = true;
            var channel = connection.CreateConnection();
            return channel;
        }
    }
}
