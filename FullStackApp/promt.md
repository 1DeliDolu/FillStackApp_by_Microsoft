
Tamam — Frontend’de **“Realtime API / Forecast API / History API”** seçeneklerinden biri seçilecek ve **seçime göre backend endpoint’i çağrılıp** uygun data gösterilecek.

Aşağıda **(1) Backend’e History ekleme** + **(2) Frontend’de seçim UI’ı + koşullu gösterim** + **(3) Test** adımlarını, kopyala-yapıştır olacak şekilde veriyorum.

> Dış servis: WeatherAPI.com
> History endpoint’inde `dt` parametresi zorunlu; `end_dt` opsiyonel (pro plan), tarih aralığı/format kısıtları var. ([WeatherAPI](https://www.weatherapi.com/docs/?utm_source=chatgpt.com "Weather and Geolocation API JSON and XML"))

---

## 1) Backend: History endpoint’i ekle (ServerApp)

### 1.1 `.env` (sen zaten var dedin)

```env
WEATHER_API_BASE_URL=http://api.weatherapi.com/v1/
WEATHER_API_KEY=YOUR_API_KEY_HERE
```

### 1.2 `ServerApp/Program.cs` içine History endpoint’i ekle

Mevcut `/api/weather/current` ve `/api/weather/forecast` yanında şunu da ekle:

```csharp
// GET /api/weather/history?q=London&dt=2022-01-01
app.MapGet("/api/weather/history", async (string q, string dt, IHttpClientFactory httpFactory, IMemoryCache cache) =>
{
    if (string.IsNullOrWhiteSpace(q))
        return Results.BadRequest(new { message = "Query parameter 'q' is required." });

    if (string.IsNullOrWhiteSpace(dt))
        return Results.BadRequest(new { message = "Query parameter 'dt' is required (yyyy-MM-dd)." });

    // Basit format kontrolü (isteğe bağlı ama faydalı)
    if (!DateOnly.TryParse(dt, out _))
        return Results.BadRequest(new { message = "dt must be a valid date in yyyy-MM-dd format." });

    // History dataları değişmez → cache'i daha uzun tutabilirsin
    var cacheKey = $"weather_history::{q.Trim().ToLowerInvariant()}::{dt}";
    if (cache.TryGetValue(cacheKey, out object? cached) && cached is not null)
        return Results.Ok(cached);

    var http = httpFactory.CreateClient("WeatherApi");
    var url = $"history.json?key={Uri.EscapeDataString(weatherApiKey)}&q={Uri.EscapeDataString(q)}&dt={Uri.EscapeDataString(dt)}";

    HttpResponseMessage resp;
    try
    {
        resp = await http.GetAsync(url);
    }
    catch (Exception ex)
    {
        return Results.Problem($"WeatherAPI request failed: {ex.Message}", statusCode: 502);
    }

    var body = await resp.Content.ReadAsStringAsync();
    if (!resp.IsSuccessStatusCode)
        return Results.Problem($"WeatherAPI error: {body}", statusCode: (int)resp.StatusCode);

    try
    {
        // History response, Forecast objesine benzer → aynı mapper mantığı
        var dto = MapForecast(body);

        cache.Set(cacheKey, dto, TimeSpan.FromHours(6));
        return Results.Ok(dto);
    }
    catch (JsonException jex)
    {
        return Results.Problem($"Malformed JSON from WeatherAPI: {jex.Message}", statusCode: 502);
    }
});
```

**Notlar**

* WeatherAPI dokümanında History için `dt` (gün) zorunlu; `end_dt` opsiyonel ve plan kısıtı var. ([WeatherAPI](https://www.weatherapi.com/docs/?utm_source=chatgpt.com "Weather and Geolocation API JSON and XML"))
* Sen şimdilik tek gün (`dt`) ile başlayınca UI/UX ve implementasyon daha temiz olur.

---

## 2) Frontend: Seçim (Realtime / Forecast / History) ve koşullu UI

Aşağıdaki örnek, tek sayfada seçim yapıp **seçime göre** doğru endpoint’i çağırır ve uygun input’ları açar:

* Realtime: sadece `q`
* Forecast: `q + days`
* History: `q + dt (date picker)`

### 2.1 `ClientApp/Pages/Weather.razor` (tam sayfa örneği)

> Mevcut Weather.razor’unu bununla değiştir veya buna göre düzenle.

```razor
@page "/weather"
@using System.Net.Http.Json
@inject HttpClient Http

<h3>Weather</h3>

<div style="display:flex; gap:12px; flex-wrap:wrap; align-items:end;">
    <div>
        <label>Mode</label><br />
        <select @bind="mode" style="min-width:200px;">
            <option value="realtime">Realtime API</option>
            <option value="forecast">Forecast API</option>
            <option value="history">History API</option>
        </select>
    </div>

    <div>
        <label>Location (q)</label><br />
        <input @bind="query" placeholder="London / 07112 / lat,long" style="min-width:260px;" />
    </div>

    @if (mode == "forecast")
    {
        <div>
            <label>Days</label><br />
            <input type="number" @bind="days" min="1" max="14" style="width:90px;" />
        </div>
    }

    @if (mode == "history")
    {
        <div>
            <label>Date (dt)</label><br />
            <input type="date" @bind="historyDate" />
        </div>
    }

    <button @onclick="Load" style="height:32px;">Get</button>
</div>

@if (!string.IsNullOrWhiteSpace(error))
{
    <p style="color:#b00020; margin-top:12px;">@error</p>
}

@if (mode == "realtime" && current is not null)
{
    <h4>Current</h4>
    <p>
        <strong>@current.LocationName</strong>, @current.Country — @current.LocalTime <br />
        @current.TempC °C, @current.ConditionText <br />
        Wind: @current.WindKph kph — Humidity: @current.Humidity% <br />
        Updated: @current.LastUpdated
    </p>
}

@if ((mode == "forecast" || mode == "history") && forecastLike is not null)
{
    <h4>@(mode == "forecast" ? $"Forecast ({forecastLike.Days.Length} days)" : "History (daily summary)")</h4>
    <p><strong>@forecastLike.LocationName</strong>, @forecastLike.Country — @forecastLike.LocalTime</p>

    <ul>
        @foreach (var d in forecastLike.Days)
        {
            <li>
                <strong>@d.Date</strong> — @d.ConditionText
                (min: @d.MinTempC °C, max: @d.MaxTempC °C, avg: @d.AvgTempC °C)
            </li>
        }
    </ul>
}

@code {
    // "realtime" | "forecast" | "history"
    private string mode = "realtime";

    private string query = "London";
    private int days = 7;

    // input type="date" → yyyy-MM-dd string
    private string historyDate = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");

    private string? error;

    private CurrentWeatherDto? current;
    private ForecastDto? forecastLike;

    private async Task Load()
    {
        error = null;
        current = null;
        forecastLike = null;

        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                error = "Please enter a location (q).";
                return;
            }

            if (mode == "realtime")
            {
                current = await Http.GetFromJsonAsync<CurrentWeatherDto>(
                    $"api/weather/current?q={Uri.EscapeDataString(query)}");
            }
            else if (mode == "forecast")
            {
                forecastLike = await Http.GetFromJsonAsync<ForecastDto>(
                    $"api/weather/forecast?q={Uri.EscapeDataString(query)}&days={days}");
            }
            else if (mode == "history")
            {
                if (string.IsNullOrWhiteSpace(historyDate))
                {
                    error = "Please select a date (dt).";
                    return;
                }

                // History endpoint expects dt=yyyy-MM-dd
                forecastLike = await Http.GetFromJsonAsync<ForecastDto>(
                    $"api/weather/history?q={Uri.EscapeDataString(query)}&dt={Uri.EscapeDataString(historyDate)}");
            }
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }

    // DTOs (backend ile aynı contract)
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
}
```

---

## 3) Test plan (hızlı)

1. Server çalıştır:

```bash
cd ServerApp
dotnet run
```

2. Client çalıştır:

```bash
cd ClientApp
dotnet run
```

3. Tarayıcı:

* `/weather`
* Mode = **Realtime API** → “London” → Get → current dolmalı
* Mode = **Forecast API** → days=7 → Get → 7 günlük liste
* Mode = **History API** → dt seç → Get → seçilen günün daily summary

4. Direkt backend test:

* `/api/weather/current?q=London`
* `/api/weather/forecast?q=London&days=7`
* `/api/weather/history?q=London&dt=2022-01-01` ([WeatherAPI](https://www.weatherapi.com/docs/?utm_source=chatgpt.com "Weather and Geolocation API JSON and XML"))

---

## 4) Küçük ama kritik notlar

* **API_KEY hiçbir zaman frontend’e gönderilmez** (backend proxy) ✅
* History API bazı planlarda kısıtlı olabilir; hata alırsan backend `Problem(...)` ile mesaj döndürür. ([WeatherAPI](https://www.weatherapi.com/docs/?utm_source=chatgpt.com "Weather and Geolocation API JSON and XML"))
* History için `dt` formatı `yyyy-MM-dd`. ([WeatherAPI](https://www.weatherapi.com/docs/?utm_source=chatgpt.com "Weather and Geolocation API JSON and XML"))

---

İstersen bir sonraki adımda şunu da ekleyebilirim:
 **History mode** ’da daily summary yanında **hourly** verileri de (WeatherAPI’nin `forecastday[0].hour[]`) çekip tablo halinde göstermek.
