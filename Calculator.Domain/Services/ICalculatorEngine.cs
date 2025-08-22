using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Domain.Services
{
    public interface ICalculatorEngine
    {
        decimal Evaluate(string expression);
    }
}
