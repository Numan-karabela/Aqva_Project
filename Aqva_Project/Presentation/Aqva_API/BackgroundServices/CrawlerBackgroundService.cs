using Application.Abstraction;

namespace Aqva_API.BackgroundServices;

public sealed class CrawlerBackgroundService : BackgroundService 
{
    private readonly ICrawlerService _webCrawler;
    private readonly ILogger<CrawlerBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public CrawlerBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<CrawlerBackgroundService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        var scope = serviceScopeFactory.CreateScope();
        _webCrawler = scope.ServiceProvider.GetRequiredService<ICrawlerService>();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _webCrawler.CrawlPageAsync(_configuration["Crawler:BaseUrl"]!);
                
                _logger.LogInformation("Crawling completed successfully");

                if (!int.TryParse(_configuration["Crawler:Time"], out var delayDays))
                {
                    delayDays = 30;
                }

                await Task.Delay(TimeSpan.FromDays(delayDays), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while crawling");
            }
        }
    }
}