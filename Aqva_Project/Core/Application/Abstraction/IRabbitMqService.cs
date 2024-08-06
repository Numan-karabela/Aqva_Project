namespace Application.Abstraction;

public interface IRabbitMqService
{
    public Task Publish<T>(string topic, T message);
}