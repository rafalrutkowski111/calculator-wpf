using Calculator.Domain.Services;
using Calculator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Data.Services;

public class EfExchangeRateStore : IExchangeRateStore
{
    private readonly AppDbContext _db;
    public EfExchangeRateStore(AppDbContext db) => _db = db;

    // wstawianie lub aktualizacja kursów do bazy
    public async Task UpsertAsync(string currency, IReadOnlyList<FxPoint> points, CancellationToken ct = default)
    {
        if (points.Count == 0) return;

        var from = points.Min(p => p.Date);
        var to = points.Max(p => p.Date);
        var existing = await _db.ExchangeRates
            .Where(x => x.Currency == currency && x.EffectiveDate >= from && x.EffectiveDate <= to)
            .ToDictionaryAsync(x => x.EffectiveDate, ct);

        foreach (var p in points)
        {
            if (existing.TryGetValue(p.Date, out var row))
                row.Rate = p.Rate;
            else
                _db.ExchangeRates.Add(new ExchangeRate { Currency = currency, EffectiveDate = p.Date, Rate = p.Rate, Source = "NBP" });
        }
        await _db.SaveChangesAsync(ct);
    }

    // pobieranie kursów walut z bazy
    public async Task<IReadOnlyList<FxPoint>> GetAsync(string currency, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var list = await _db.ExchangeRates
            .Where(x => x.Currency == currency && x.EffectiveDate >= from && x.EffectiveDate <= to)
            .OrderBy(x => x.EffectiveDate)
            .Select(x => new FxPoint(x.EffectiveDate, x.Rate))
            .ToListAsync(ct);
        return list;
    }
}
