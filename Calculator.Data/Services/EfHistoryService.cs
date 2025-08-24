using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator.Data.Entities;
using Calculator.Domain.Models;
using Calculator.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Calculator.Data.Services;

public class EfHistoryService : IHistoryService
{
    private readonly AppDbContext _db;
    public EfHistoryService(AppDbContext db) => _db = db;

    public async Task SaveAsync(string expression, string result, DateTime whenUtc)
    {
        _db.Calculations.Add(new Calculation
        {
            Expression = expression,
            Result = result,
            CreatedAtUtc = whenUtc
        });
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<CalculationDto>> GetLastAsync(int count)
    {
        return await _db.Calculations
            .OrderByDescending(x => x.Id)
            .Take(count)
            .Select(x => new CalculationDto(x.Id, x.Expression, x.Result, x.CreatedAtUtc))
            .ToListAsync();
    }

    // tutaj dostajemy x wybranych wpisów jak w GetLastAsync, ale z filtrem
    // czyli najpierw filtrujemy z bazy a potem pobieramy (nie że lokalnie filtrujemy co pobralismy)
    // ławtiej było napisać to w sql niż ef
    public async Task<IReadOnlyList<CalculationDto>> SearchAsync(string query, int limit)
    {
        query ??= string.Empty;
        var like = $"%{query}%";

        var rows = await _db.Calculations
            .FromSqlInterpolated($@"
            SELECT *
            FROM Calculations
            WHERE Expression LIKE {like}
               OR Result     LIKE {like}
               OR strftime('%Y-%m-%d', CreatedAtUtc, 'localtime') = {query}
               OR strftime('%Y-%m',     CreatedAtUtc, 'localtime') = {query}
            ORDER BY Id DESC
            LIMIT {limit}
        ")
            .AsNoTracking()
            .ToListAsync();

        return rows.Select(x => new CalculationDto(x.Id, x.Expression, x.Result, x.CreatedAtUtc)).ToList();
    }

}
