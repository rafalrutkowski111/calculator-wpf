using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Domain.Services;

public record FxPoint(DateOnly Date, decimal Rate);

public interface IExchangeRateFetcher
{
    Task<IReadOnlyList<FxPoint>> FetchAsync(string currency, DateOnly from, DateOnly to, CancellationToken ct = default);
}

public interface IExchangeRateStore
{
    Task UpsertAsync(string currency, IReadOnlyList<FxPoint> points, CancellationToken ct = default);
    Task<IReadOnlyList<FxPoint>> GetAsync(string currency, DateOnly from, DateOnly to, CancellationToken ct = default);
}

public enum FxStrategy { Max, Min }

public interface IExchangeAdvisor
{
    (DateOnly bestDate, decimal rate, decimal converted) ChooseBest(
        decimal amount, FxStrategy strategy, IReadOnlyList<FxPoint> points);
}

public class ExchangeAdvisor : IExchangeAdvisor
{
    public (DateOnly, decimal, decimal) ChooseBest(decimal amount, FxStrategy strategy, IReadOnlyList<FxPoint> points)
    {
        if (points == null || points.Count == 0) throw new InvalidOperationException("Brak danych kursowych.");
        var best = strategy == FxStrategy.Max
            ? points.MaxBy(p => p.Rate)!
            : points.MinBy(p => p.Rate)!;

        var converted = amount * best.Rate;
        return (best.Date, best.Rate, decimal.Round(converted, 2));
    }
}
