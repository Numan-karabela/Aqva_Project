using MudBlazor.Services;
using Aqva_Blazor.Client.Pages;
using Aqva_Blazor.Client.Services;
using Aqva_Blazor.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMudServices();
builder.Services.AddHttpClient();

builder.Services.AddScoped<ColumnistService>(); 

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

app.UseExceptionHandler("/Error", createScopeForErrors: true);
app.UseHsts();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Aqva_Blazor.Client._Imports).Assembly);

await app.RunAsync();