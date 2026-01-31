using System.Text.Json;
using DotNetEnv;
using Microsoft.Extensions.Caching.Memory;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ClientCors",
        policy =>
            policy.WithOrigins("http://localhost:5181").AllowAnyHeader().AllowAnyMethod()
    );
});

// Performance: cache
builder.Services.AddMemoryCache();

// JSON naming
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Read config from environment
var weatherBaseUrl =
    builder.Configuration["WEATHER_API_BASE_URL"] ?? "https://api.weatherapi.com/v1/";
if (!weatherBaseUrl.EndsWith("/", StringComparison.Ordinal))
{
    weatherBaseUrl += "/";
}

var weatherApiKey = builder.Configuration["WEATHER_API_KEY"];
if (string.IsNullOrWhiteSpace(weatherApiKey))
{
    throw new InvalidOperationException(
        "WEATHER_API_KEY is missing. Put it into .env (and do not commit it)."
    );
}

var weatherApiKeyValue = weatherApiKey.Trim();

// HttpClient for WeatherAPI
builder.Services.AddHttpClient(
    "WeatherApi",
    client =>
    {
        client.BaseAddress = new Uri(weatherBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(10);
    }
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ClientCors");

app.MapControllers();

// ✅ Doğru endpoint: /api/productlist
app.MapGet(
    "/api/productlist",
    static () =>
    {
        // ✅ Geçerli JSON döndüren örnek veri
        return Results.Ok(
            new[]
            {
                new ProductDto(1, "Laptop", 1200.50, 25, new CategoryDto(1, "Electronics")),
                new ProductDto(2, "Headphones", 50.00, 100, new CategoryDto(1, "Electronics")),
                new ProductDto(3, "Keyboard", 75.99, 50, new CategoryDto(2, "Accessories")),
                new ProductDto(4, "Mouse", 40.00, 150, new CategoryDto(2, "Accessories")),
            }
        );
    }
)
    .WithOpenApi();

// GET /api/weather/current?q=London
app.MapGet(
    "/api/weather/current",
    async (string q, IHttpClientFactory httpFactory, IMemoryCache cache) =>
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.BadRequest(
                new { message = "Query parameter 'q' is required (city, zip, or lat,long)." }
            );
        }

        var trimmedQuery = q.Trim();
        var cacheKey = $"weather_current::{trimmedQuery.ToLowerInvariant()}";
        if (cache.TryGetValue(cacheKey, out CurrentWeatherDto? cached) && cached is not null)
        {
            return Results.Ok(cached);
        }

        var http = httpFactory.CreateClient("WeatherApi");
        var url =
            $"current.json?key={Uri.EscapeDataString(weatherApiKeyValue)}&q={Uri.EscapeDataString(trimmedQuery)}";

        HttpResponseMessage resp;
        try
        {
            resp = await http.GetAsync(url);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                $"WeatherAPI request failed: {ex.Message}",
                statusCode: StatusCodes.Status502BadGateway
            );
        }

        var body = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            return Results.Problem($"WeatherAPI error: {body}", statusCode: (int)resp.StatusCode);
        }

        try
        {
            var dto = MapCurrentWeather(body);
            cache.Set(cacheKey, dto, TimeSpan.FromSeconds(60));
            return Results.Ok(dto);
        }
        catch (JsonException jex)
        {
            return Results.Problem(
                $"Malformed JSON from WeatherAPI: {jex.Message}",
                statusCode: StatusCodes.Status502BadGateway
            );
        }
    }
)
    .WithOpenApi();

// GET /api/weather/forecast?q=London&days=7
app.MapGet(
    "/api/weather/forecast",
    async (string q, IHttpClientFactory httpFactory, IMemoryCache cache, int days = 7) =>
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.BadRequest(new { message = "Query parameter 'q' is required." });
        }

        if (days <= 0 || days > 14)
        {
            return Results.BadRequest(new { message = "days must be between 1 and 14." });
        }

        var trimmedQuery = q.Trim();
        var cacheKey = $"weather_forecast::{trimmedQuery.ToLowerInvariant()}::{days}";
        if (cache.TryGetValue(cacheKey, out ForecastDto? cached) && cached is not null)
        {
            return Results.Ok(cached);
        }

        var http = httpFactory.CreateClient("WeatherApi");
        var url =
            $"forecast.json?key={Uri.EscapeDataString(weatherApiKeyValue)}&q={Uri.EscapeDataString(trimmedQuery)}&days={days}";

        HttpResponseMessage resp;
        try
        {
            resp = await http.GetAsync(url);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                $"WeatherAPI request failed: {ex.Message}",
                statusCode: StatusCodes.Status502BadGateway
            );
        }

        var body = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            return Results.Problem($"WeatherAPI error: {body}", statusCode: (int)resp.StatusCode);
        }

        try
        {
            var dto = MapForecast(body);
            cache.Set(cacheKey, dto, TimeSpan.FromMinutes(10));
            return Results.Ok(dto);
        }
        catch (JsonException jex)
        {
            return Results.Problem(
                $"Malformed JSON from WeatherAPI: {jex.Message}",
                statusCode: StatusCodes.Status502BadGateway
            );
        }
    }
)
    .WithOpenApi();

app.Run();

static CurrentWeatherDto MapCurrentWeather(string json)
{
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    var location = root.GetProperty("location");
    var current = root.GetProperty("current");
    var condition = current.GetProperty("condition");

    return new CurrentWeatherDto(
        LocationName: location.GetProperty("name").GetString() ?? string.Empty,
        Country: location.GetProperty("country").GetString() ?? string.Empty,
        LocalTime: location.GetProperty("localtime").GetString() ?? string.Empty,
        TempC: current.GetProperty("temp_c").GetDouble(),
        ConditionText: condition.GetProperty("text").GetString() ?? string.Empty,
        WindKph: current.GetProperty("wind_kph").GetDouble(),
        Humidity: current.GetProperty("humidity").GetInt32(),
        LastUpdated: current.GetProperty("last_updated").GetString() ?? string.Empty
    );
}

static ForecastDto MapForecast(string json)
{
    using var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    var location = root.GetProperty("location");
    var forecast = root.GetProperty("forecast");
    var days = forecast.GetProperty("forecastday");

    var forecastDays = new List<ForecastDayDto>(days.GetArrayLength());
    foreach (var d in days.EnumerateArray())
    {
        var day = d.GetProperty("day");
        var condition = day.GetProperty("condition");

        forecastDays.Add(
            new ForecastDayDto(
                Date: d.GetProperty("date").GetString() ?? string.Empty,
                MaxTempC: day.GetProperty("maxtemp_c").GetDouble(),
                MinTempC: day.GetProperty("mintemp_c").GetDouble(),
                AvgTempC: day.GetProperty("avgtemp_c").GetDouble(),
                ConditionText: condition.GetProperty("text").GetString() ?? string.Empty
            )
        );
    }

    return new ForecastDto(
        LocationName: location.GetProperty("name").GetString() ?? string.Empty,
        Country: location.GetProperty("country").GetString() ?? string.Empty,
        LocalTime: location.GetProperty("localtime").GetString() ?? string.Empty,
        Days: forecastDays.ToArray()
    );
}

public sealed record CurrentWeatherDto(
    string LocationName,
    string Country,
    string LocalTime,
    double TempC,
    string ConditionText,
    double WindKph,
    int Humidity,
    string LastUpdated
);

public sealed record ForecastDto(
    string LocationName,
    string Country,
    string LocalTime,
    ForecastDayDto[] Days
);

public sealed record ForecastDayDto(
    string Date,
    double MaxTempC,
    double MinTempC,
    double AvgTempC,
    string ConditionText
);

record ProductDto(int Id, string Name, double Price, int Stock, CategoryDto Category);

record CategoryDto(int Id, string Name);
