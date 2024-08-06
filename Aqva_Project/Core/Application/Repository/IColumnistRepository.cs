using Domain.Entity;

namespace Application.Repository;

public interface IColumnistRepository
{
    public Task CreateNewIndexAsync();
    public Task IndexDocumentAsync(CrawledPage page);
    public Task<IEnumerable<Columnist>> SearchAsync(string query);
    public Task<IEnumerable<Columnist>> GetAllColumnistsAsync();
}