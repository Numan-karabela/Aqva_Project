using Domain.Entity;

namespace Application.Abstraction;

public interface IWebCrawler
{
    public Task<CrawledPage> CrawlPageAsync(string url);
}