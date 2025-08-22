using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Domain.Exceptions;

public class InvalidExpressionException : Exception
{
    public InvalidExpressionException(string message) : base(message) { }
}

public class DivisionByZeroDomainException : Exception
{
    public DivisionByZeroDomainException() : base("Division by zero.") { }
}
