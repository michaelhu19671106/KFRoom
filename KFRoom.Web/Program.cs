using KFRoom.Web;
using KFRoom.Web.Components;
using KFRoom.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.Services.AddBlazorBootstrap();


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();
// 註冊HttpClient服務和配置服務
builder.Services.AddHttpClient("ApiServiceClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7432");
});

//註冊Service服務並注入剛註冊的HttpClient服務和配置服務
builder.Services.AddScoped<AccountService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new AccountService(httpClientFactory.CreateClient("ApiServiceClient"), configuration);
});

//// 註冊 SessionStorage 服務
//builder.Services.AddSessionStorageServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
