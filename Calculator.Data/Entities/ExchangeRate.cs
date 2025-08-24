using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Data.Entities;

public class ExchangeRate
{
    public int Id { get; set; }
    public string Currency { get; set; } = default!;
    public DateOnly EffectiveDate { get; set; }
    public decimal Rate { get; set; }
    public string Source { get; set; } = "NBP";
}

