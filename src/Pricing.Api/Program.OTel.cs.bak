using OpenTelemetry.Trace;
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
            .AddSentry();
    });

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

var basePrices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
{
    ["Wireless Headphones"]      = 89.99m,
    ["Mechanical Keyboard"]      = 129.00m,
    ["USB-C Hub"]                = 39.50m,
    ["Monitor Stand"]            = 49.99m,
    ["Webcam HD 4K"]             = 119.00m,
    ["Laptop Stand Aluminum"]    = 59.00m,
    ["XL Mouse Pad"]             = 19.99m,
    ["Cable Management Kit"]     = 14.50m,
    ["LED Desk Lamp"]            = 44.00m,
    ["Ergonomic Chair Cushion"]  = 34.99m,
    ["Bluetooth Speaker"]        = 69.00m,
    ["Phone Holder"]             = 12.99m,
    ["Surge Protector 6-port"]   = 24.99m,
    ["Screen Cleaner Kit"]       = 9.99m,
    ["Desk Organizer"]           = 29.00m,
    ["Trackpad Pro"]             = 99.00m,
    ["Wrist Rest Gel"]           = 17.50m,
    ["Privacy Screen Filter"]    = 39.99m,
    ["Portable SSD 1TB"]         = 109.00m,
    ["Docking Station"]          = 179.00m,
};

decimal Resolve(string product) =>
    basePrices.TryGetValue(product, out var p) ? p : 19.99m;

var prices = app.MapGroup("/api/prices").WithTags("Pricing");

prices.MapGet("/{productName}", async (string productName) =>
{
    await Task.Delay(Random.Shared.Next(15, 30));

    return TypedResults.Ok(new PriceResponse(
        ProductName: productName,
        CurrentPrice: Resolve(productName),
        DiscountPercent: 0));
});

prices.MapPost("/batch", async (BatchRequest request) =>
{
    // One round trip, regardless of how many products are requested
    await Task.Delay(Random.Shared.Next(20, 40));

    var result = request.ProductNames
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Select(name => new PriceResponse(name, Resolve(name), 0))
        .ToList();

    return TypedResults.Ok(result);
});

app.Run();

public record PriceResponse(string ProductName, decimal CurrentPrice, int DiscountPercent);
public record BatchRequest(List<string> ProductNames);
