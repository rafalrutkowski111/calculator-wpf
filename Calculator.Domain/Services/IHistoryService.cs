using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator.Domain.Models;

namespace Calculator.Domain.Services;

public interface IHistoryService
{
    Task SaveAsync(string expression, string result, DateTime whenUtc);
    Task<IReadOnlyList<CalculationDto>> GetLastAsync(int count);
}
