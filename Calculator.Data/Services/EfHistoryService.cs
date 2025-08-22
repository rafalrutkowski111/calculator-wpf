using System;
using System.Collections.Generic;
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
}
