namespace Domain.Entity
{ 
    public sealed class Columnist : BaseEntity
    {
        public required string Name { get; init; }

        public required string ArticleTitle { get; init; }

        public required string PublishDate { get; init; } 
    }
    public sealed class CrawledPage : BaseEntity
    {
        public List<Columnist> Columnists { get; set; } = [];
    }
}