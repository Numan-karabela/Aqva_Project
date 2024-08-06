using System.Text;
using Domain.Entity;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Aqva_API.Consumer;

public sealed class RabbitMqConsumer : IDisposable
{
    private readonly IModel _channel;
    private readonly IConnection _connection;

    public RabbitMqConsumer(IConfiguration configuration)
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

    public void StartConsuming(string queueName, Func<CrawledPage, Task> messageHandler)
    {
        _channel.QueueDeclare(queueName, false, false, false, null);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Error = (_, args) =>
                    {
                        Console.WriteLine($"JSON Deserialization Error: {args.ErrorContext.Error.Message}");
                        args.ErrorContext.Handled = true;
                    }
                };

                var deserializedMessage = JsonConvert.DeserializeObject<Columnist>(message, jsonSerializerSettings);
                if (deserializedMessage != null)
                {
                    var crawledPage = new CrawledPage
                    {
                        Columnists = [deserializedMessage],
                        Id = deserializedMessage.Id
                    };
                    await messageHandler(crawledPage);
                }
                else
                {
                    Console.WriteLine("The message could not be deserialized.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while processing the message: {ex.Message}");
            }
        };

        _channel.BasicConsume(queueName, true, consumer);
        Console.WriteLine($"Consumer started for queue: {queueName}");
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}