using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ClientApp.Models;
using Microsoft.Extensions.Configuration;

namespace ClientApp.Services
{
    public sealed class ProductService
    {
        private readonly HttpClient _http;
        private Product[]? _cachedProducts;
        private DateTimeOffset _cacheTime;
        private readonly TimeSpan _cacheDuration;

        public ProductService(HttpClient http, IConfiguration configuration)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            var seconds = configuration.GetValue<int?>("CacheSettings:ProductListSeconds") ?? 30;
            _cacheDuration = seconds > 0 ? TimeSpan.FromSeconds(seconds) : TimeSpan.Zero;
        }

        public async Task<Product[]?> GetProductsAsync(CancellationToken cancellationToken = default)
        {
            if (_cacheDuration > TimeSpan.Zero
                && _cachedProducts is not null
                && DateTimeOffset.UtcNow - _cacheTime < _cacheDuration)
            {
                return _cachedProducts;
            }

            // Copilot suggestion: add a short-lived client cache to reduce redundant API calls.
            var response = await _http.GetAsync("/api/productlist", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var products = JsonSerializer.Deserialize<Product[]>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (products is not null)
            {
                _cachedProducts = products;
                _cacheTime = DateTimeOffset.UtcNow;
            }

            return products;
        }

        public void ClearCache()
        {
            _cachedProducts = null;
        }
    }
}
