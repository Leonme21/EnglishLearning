using EnglishLearning.Client;
using EnglishLearning.Client.Services;
using EnglishLearning.Client.Services.Api;
using EnglishLearning.Client.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var configuredUrl = builder.Configuration["ApiBaseUrl"];
var apiUrl = !string.IsNullOrWhiteSpace(configuredUrl) && (builder.HostEnvironment.IsDevelopment() || !configuredUrl.Contains("localhost"))
    ? configuredUrl
    : builder.HostEnvironment.BaseAddress;

// Auth & Storage
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddAuthorizationCore();

// State & UI Services
builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AssessmentService>();
builder.Services.AddScoped<ProgressService>();
builder.Services.AddScoped<LessonService>();
builder.Services.AddScoped<ChatService>();

// HTTP Clients
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddHttpClient("AuthClient", client => 
{
    client.BaseAddress = new Uri(apiUrl);
});

builder.Services.AddHttpClient("DefaultClient", client => 
{
    client.BaseAddress = new Uri(apiUrl);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped(sp => 
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var client = factory.CreateClient("DefaultClient");
    return new ApiClient(client);
});

await builder.Build().RunAsync();
