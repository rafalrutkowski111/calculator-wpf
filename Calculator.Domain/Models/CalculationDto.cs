using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Domain.Models;

public record CalculationDto(int Id, string Expression, string Result, DateTime CreatedAtUtc);
