using Application.Abstraction;
using Application.Repository;
using Aqva_API.BackgroundServices;
using Aqva_API.Consumer;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Infrastructure.Crawler;
using Infrastructure.RabbitMq;
using Persistance.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<IndexerBackgroundService>();
builder.Services.AddHostedService<CrawlerBackgroundService>();

builder.Services.AddSingleton<IWebCrawler, WebCrawler>();
builder.Services.AddSingleton<IColumnistRepository, ColumnistRepository>(_ =>
{
    var settings = new ElasticsearchClientSettings(new Uri(builder.Configuration["Elasticsearch:Uri"]!))
        .Authentication(new BasicAuthentication(builder.Configuration["Elasticsearch:Username"]!, builder.Configuration["Elasticsearch:Password"]!))
        .PrettyJson()
        .CertificateFingerprint(builder.Configuration["Elasticsearch:CertificateFingerprint"]!)
        .DisableDirectStreaming()
        .DefaultIndex(builder.Configuration["Elasticsearch:DefaultIndex"]!);
    
    return new ColumnistRepository(builder.Configuration, new ElasticsearchClient(settings));
});

builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddSingleton<RabbitMqConsumer>();
builder.Services.AddSingleton<ICrawlerService, CrawlerService>();

builder.Services.AddControllers(
    options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
