using System.Runtime.CompilerServices;
using Application.Repository;
using Domain.Entity;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;

namespace Persistance.Repository;

public sealed class ColumnistRepository(IConfiguration configuration, ElasticsearchClient elasticClient) : IColumnistRepository
{
    private readonly string _indexName = configuration["Elasticsearch:DefaultIndex"] ?? "columnists";

    public async Task CreateNewIndexAsync()
    {
        var exists = await elasticClient.Indices.ExistsAsync(_indexName);
        if (exists.Exists)
            return;

        var createIndex = await elasticClient.Indices.CreateAsync(_indexName, c =>
        {
            c.Mappings(m =>
            {
                m.Properties<Columnist>(descriptor =>
                {
                    descriptor.Keyword(x => x.Name);
                    descriptor.Text(x => x.ArticleTitle);
                    descriptor.Text(x => x.PublishDate);
                    descriptor.LongNumber(x => x.Id);
                });
            });
        });

        if (!createIndex.IsValidResponse)
            throw new Exception($"Failed to create index: {createIndex.DebugInformation}");
    }

    public async Task IndexDocumentAsync(CrawledPage page)
    {
        var indexResponse = await elasticClient.IndexAsync(page, idx => idx
            .Index(_indexName)
            .Document(page)
            .Refresh(Refresh.WaitFor)
        );

        if (!indexResponse.IsValidResponse)
            throw new Exception($"Failed to index document: {indexResponse.DebugInformation}");
    }

    public async Task<IEnumerable<Columnist>> SearchAsync(string query)
    {
        if (string.IsNullOrEmpty(query) || query.Length < 3)
            return Array.Empty<Columnist>();

        return await SearchInternalAsync(query);
    }

    private async Task<IEnumerable<Columnist>> SearchInternalAsync(string query)
    {
        try
        {
            var exactMatchTask = SearchColumnistsAsync(query, true);
            var partialMatchTask = SearchColumnistsAsync(query, false);

            await Task.WhenAll(exactMatchTask, partialMatchTask);

            var columnists = new HashSet<Columnist>(exactMatchTask.Result);
            columnists.UnionWith(partialMatchTask.Result);

            return columnists;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Array.Empty<Columnist>();
        }
    }

    private async Task<IEnumerable<Columnist>> SearchColumnistsAsync(string name, bool exactMatch)
    {
        var response = await (exactMatch ? ExactMatchSearchAsync(name) : PartialMatchSearchAsync(name));
        return response.Hits
            .SelectMany(hit => hit.Source?.Columnists ?? Enumerable.Empty<Columnist>())
            .Distinct();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Task<SearchResponse<CrawledPage>> ExactMatchSearchAsync(string name) =>
        elasticClient.SearchAsync<CrawledPage>(s => s
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                            .Term(t => t.Field(f => (f.Columnists.First().Name ?? string.Empty).Suffix("keyword"))
                                .Value(name)),
                        m => m.Term(t =>
                            t.Field(f => (f.Columnists.First().ArticleTitle ?? string.Empty).Suffix("keyword"))
                                .Value(name)),
                        m => m.Term(t =>
                            t.Field(f => (f.Columnists.First().PublishDate ?? string.Empty).Suffix("keyword"))
                                .Value(name))
                    )
                )
            )
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Task<SearchResponse<CrawledPage>> PartialMatchSearchAsync(string name) =>
        elasticClient.SearchAsync<CrawledPage>(s => s
            .Query(q => q
                .Bool(b => b
                    .Should(
                        m => m.MatchPhrasePrefix(mpp => mpp.Field(f => f.Columnists.First().Name).Query(name)),
                        m => m.MatchPhrasePrefix(mpp => mpp.Field(f => f.Columnists.First().ArticleTitle).Query(name)),
                        m => m.MatchPhrasePrefix(mpp => mpp.Field(f => f.Columnists.First().PublishDate).Query(name))
                    )
                )
            )
        );

    public async Task<IEnumerable<Columnist>> GetAllColumnistsAsync()
    {
        var columnists = new List<Columnist>();

        var initialResponse = await elasticClient.SearchAsync<CrawledPage>(s => s
            .Index(_indexName)
            .Query(q => q
                .MatchAll(x => x.QueryName("match_all"))
            )
            .Size(1000)
            .Scroll("5m")
        );

        if (!initialResponse.IsValidResponse)
            throw new Exception($"Failed to retrieve documents: {initialResponse.DebugInformation}");

        columnists.AddRange(
            initialResponse.Hits.SelectMany(hit => hit.Source?.Columnists ?? Enumerable.Empty<Columnist>()));

        var scrollId = initialResponse.ScrollId;

        while (!string.IsNullOrEmpty(scrollId!.ToString()))
        {
            var scrollRequest = new ScrollRequest
            {
                Scroll = "5m",
                ScrollId = scrollId
            };

            var scrollResponse = await elasticClient.ScrollAsync<CrawledPage>(scrollRequest);

            if (!scrollResponse.IsValidResponse)
                throw new Exception($"Failed to retrieve documents: {scrollResponse.DebugInformation}");

            columnists.AddRange(
                scrollResponse.Hits.SelectMany(hit => hit.Source?.Columnists ?? Enumerable.Empty<Columnist>()));

            scrollId = scrollResponse.ScrollId;

            if (scrollResponse.Hits.Count == 0)
                break;
        }

        await elasticClient.ClearScrollAsync(cs => cs.ScrollId(scrollId!));
        
        return columnists.Distinct().ToList();
        
    }
}
