using Application.Repository;
using Aqva_API.Consumer;
using Domain.Entity;
using Newtonsoft.Json;

namespace Aqva_API.BackgroundServices;

public sealed class IndexerBackgroundService(
    IColumnistRepository columnistRepository,
    RabbitMqConsumer rabbitMqConsumer)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await columnistRepository.CreateNewIndexAsync();

        rabbitMqConsumer.StartConsuming("crawled_pages",
            async message =>
            {
                foreach (var columnist in message.Columnists.Select(x => new CrawledPage
                         {
                             Columnists =
                             [
                                 new Columnist
                                 {
                                     Name = x.Name,
                                     ArticleTitle = x.ArticleTitle,
                                     PublishDate = x.PublishDate,
                                     Id = x.Id
                                 }
                             ],
                             Id = DateTime.UtcNow.Ticks
                         }))
                {
                    Console.WriteLine("Indexing Columnists: " + JsonConvert.SerializeObject(columnist));

                    await columnistRepository.IndexDocumentAsync(columnist);
                }

                Console.WriteLine("Indexing Columnists Completed");
            });

        await Task.CompletedTask;
    }
}