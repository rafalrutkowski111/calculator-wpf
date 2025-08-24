using Calculator.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Calculator.App.Services;

public class NbpExchangeRateFetcher : IExchangeRateFetcher
{
    private readonly HttpClient _http;
    public NbpExchangeRateFetcher(HttpClient http) => _http = http;

    private sealed class NbpResponse
    {
        public List<NbpRate> rates { get; set; } = new();
    }
    private sealed class NbpRate
    {
        public string effectiveDate { get; set; } = default!;
        public decimal mid { get; set; }
    }

    // pobiera kursy w zakresie od do
    public async Task<IReadOnlyList<FxPoint>> FetchAsync(string currency, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        // API: /api/exchangerates/rates/A/{code}/{startDate}/{endDate}/?format=json
        var url = $"https://api.nbp.pl/api/exchangerates/rates/A/{currency}/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}/?format=json";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.TryAddWithoutValidation("Accept", "application/json");

        using var resp = await _http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode)
        {
            // poza zakresem zwroc pusta liste
            return Array.Empty<FxPoint>();
        }

        await using var s = await resp.Content.ReadAsStreamAsync(ct);
        var data = await JsonSerializer.DeserializeAsync<NbpResponse>(s, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct)
                   ?? new NbpResponse();

        return data.rates
            .Select(r => new FxPoint(DateOnly.Parse(r.effectiveDate), r.mid))
            .OrderBy(p => p.Date)
            .ToList();
    }
}