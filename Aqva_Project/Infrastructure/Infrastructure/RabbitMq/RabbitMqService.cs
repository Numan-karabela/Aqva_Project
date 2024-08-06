using System.Text;
using Application.Abstraction;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Infrastructure.RabbitMq;

public sealed class RabbitMqService : IRabbitMqService, IDisposable
{
    private readonly IModel _channel;
    private readonly IConnection _connection;
    
    public RabbitMqService(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:HostName"],
            UserName = configuration["RabbitMq:UserName"],
            Password = configuration["RabbitMq:Password"],
            Port = int.Parse(configuration["RabbitMq:Port"]!)
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }
    
    public Task Publish<T>(string topic, T message)
    {
        _channel.QueueDeclare(queue: topic, durable: false, exclusive: false, autoDelete: false, arguments: null);
        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        
        _channel.BasicPublish(string.Empty, topic, null, body);
        
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}