using Web.Components;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

var sentryDsn = builder.Configuration["Sentry:Dsn"];

    builder.WebHost.UseSentry(o =>
    {
        o.Dsn = sentryDsn;
        o.TracesSampleRate = 1.0;
        o.Environment = builder.Environment.EnvironmentName;
        o.Release = "web@1.0.0";
    });

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<OrdersApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Orders"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSingleton(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.MapStaticAssets();
app.UseAntiforgery();
if (!string.IsNullOrWhiteSpace(sentryDsn) && !sentryDsn.StartsWith("YOUR_"))
    app.UseSentryTracing();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
