using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using Orders.Api.Data;
using Orders.Api.Endpoints;
using Orders.Api.Pricing;
using Scalar.AspNetCore;
using Sentry.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.TracesSampleRate = 1.0;
    options.UseOpenTelemetry();
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation(o =>
            {
                o.SetDbStatementForText = true;
            })
            .AddSentry();
    });

builder.Services.AddDbContext<OrdersDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddHttpClient<IPricingClient, PricingClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Pricing"]!);
    client.Timeout     = TimeSpan.FromSeconds(30);
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await DatabaseSeeder.SeedAsync(db);
}

app.UseCors();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapOrderEndpoints();

app.Run();
