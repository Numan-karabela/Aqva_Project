using System.Globalization;
using System.Net;
using Application.Abstraction;
using Domain.Entity;
using HtmlAgilityPack;

namespace Infrastructure.Crawler;

public sealed class WebCrawler(HttpClient httpClient) : IWebCrawler
{
    private static readonly CultureInfo CultureInfo = new("tr-TR");

    private static readonly string[] DateFormats =
        ["MM/dd/yyyy", "M/d/yyyy", "dd MMMM yyyy", "dd MMMM yyyy HH:mm:ss tt"];

    public async Task<CrawledPage> CrawlPageAsync(string url)
    {
        try
        {
            var html = await httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var crawledPage = new CrawledPage();
            var authorNodes = doc.DocumentNode.SelectNodes("//a[contains(@class, 'author')]");

            if (authorNodes == null)
            {
                Console.WriteLine("No authors found.");
                return crawledPage;
            }

            foreach (var authorNode in authorNodes)
            {
                var nameNode = authorNode.SelectSingleNode(".//span[contains(@class, 'author-name')]");
                var titleNode = authorNode.SelectSingleNode(".//span[contains(@class, 'author-content-title')]");
                var dateNode = authorNode.SelectSingleNode(".//span[contains(@class, 'small text-secondary')]");

                if (nameNode == null || titleNode == null || dateNode == null)
                {
                    Console.WriteLine("Some elements were not found in the node.");
                    continue;
                }

                var name = WebUtility.HtmlDecode(nameNode.InnerText.Trim());
                var articleTitle = WebUtility.HtmlDecode(titleNode.InnerText.Trim());
                var publishDate = ExtractDate(dateNode.InnerText.Trim());

                crawledPage.Columnists.Add(new Columnist
                {
                    Name = name,
                    ArticleTitle = articleTitle,
                    PublishDate = publishDate,
                    Id = DateTime.UtcNow.Ticks
                });
            }

            return crawledPage;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred while crawling the page: {e.Message}");
            return new CrawledPage();
        }
    }

    private static string ExtractDate(string dateString)
    {
        var decodedDateString = WebUtility.HtmlDecode(dateString).Trim();

        if (DateTime.TryParseExact(decodedDateString, DateFormats, CultureInfo, DateTimeStyles.None, out var date))
        {
            return date.ToString("dd MMMM yyyy", CultureInfo);
        }

        throw new FormatException($"Date string '{decodedDateString}' could not be parsed.");
    }
}