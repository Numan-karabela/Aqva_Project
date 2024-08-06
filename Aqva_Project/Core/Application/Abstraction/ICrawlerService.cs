namespace Application.Abstraction;

public interface ICrawlerService
{
    public Task CrawlPageAsync(string baseUrl);
}