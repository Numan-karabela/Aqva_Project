using Application.Abstraction;

namespace Infrastructure.Crawler;

public sealed class CrawlerService(IWebCrawler webCrawler, IRabbitMqService rabbitMqService) : ICrawlerService
{
    public async Task CrawlPageAsync(string baseUrl)
    {
        try
        {
            Console.WriteLine($"Starting to crawl page: {baseUrl}");
            var crawledPage = await webCrawler.CrawlPageAsync(baseUrl);
            
            foreach (var columnist in crawledPage.Columnists)
            {
                await rabbitMqService.Publish("crawled_pages", columnist);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred while crawling page {baseUrl}: {e.Message}");
        }
    }
}