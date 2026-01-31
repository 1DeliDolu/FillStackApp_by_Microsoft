using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseCors(policy =>
    policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

app.MapGet("/api/productlist", (IMemoryCache cache, IConfiguration configuration) =>
{
    // Copilot suggestion: use a short-lived server cache to reduce repeated allocations.
    return cache.GetOrCreate("productlist", entry =>
    {
        var seconds = configuration.GetValue<int?>("CacheSettings:ProductListSeconds") ?? 30;
        entry.AbsoluteExpirationRelativeToNow = seconds > 0
            ? TimeSpan.FromSeconds(seconds)
            : TimeSpan.Zero;
        return new[]
        {
            new ProductDto(1, "Laptop", 1200.50, 25, new CategoryDto(101, "Electronics")),
            new ProductDto(2, "Headphones", 50.00, 100, new CategoryDto(102, "Accessories"))
        };
    });
});

app.Run();

internal sealed record CategoryDto(int Id, string Name);
internal sealed record ProductDto(int Id, string Name, double Price, int Stock, CategoryDto Category);
