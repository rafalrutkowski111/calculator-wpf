using Calculator.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator.Domain.Services
{
    // klasa ta umożliwia wklejanie działań do obliczeń, ale obsługuje tylko "a op b"
    public class BasicCalculatorEngine : ICalculatorEngine
    {
        // moze dodać potem możliwość wklejania większych operacji np "4 + 6 + 9" 
        public decimal Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new DomainInvalidExpressionException("Empty expression.");

            var cultureInfo = CultureInfo.InvariantCulture;
            var cleanedExpression = new string(expression.Where(c => !char.IsWhiteSpace(c)).ToArray());
            cleanedExpression = cleanedExpression.Replace(',', '.');

            if (cleanedExpression.Length == 0)
                throw new DomainInvalidExpressionException("Empty expression.");

            //szukamy operatora i jego indexu. Zaczynamy od i=1, aby wykluczyć ewentualny - przed pierwszą liczbą
            int operatorIndex = -1;
            char operatorChar = '\0';
            for (int i = 1; i < cleanedExpression.Length; i++)
            {
                char ch = cleanedExpression[i];
                if (ch is '+' or '-' or '*' or '/')
                {
                    operatorIndex = i;
                    operatorChar = ch;
                    break;
                }
            }

            if (operatorIndex <= 0 || operatorIndex >= cleanedExpression.Length - 1)
                throw new DomainInvalidExpressionException($"Invalid expression: {expression}");

            var leftNumber = cleanedExpression[..operatorIndex];
            var rightNumber = cleanedExpression[(operatorIndex + 1)..];

            if (!decimal.TryParse(leftNumber, NumberStyles.Number, cultureInfo, out var left))
                throw new DomainInvalidExpressionException($"Invalid left operand: {leftNumber}");

            if (!decimal.TryParse(rightNumber, NumberStyles.Number, cultureInfo, out var right))
                throw new DomainInvalidExpressionException($"Invalid right operand: {rightNumber}");


            return operatorChar switch
            {
                '+' => left + right,
                '-' => left - right,
                '*' => left * right,
                '/' => right == 0 ? throw new DivisionByZeroDomainException() : left / right,
                _ => throw new DomainInvalidExpressionException($"Unsupported operator: {operatorChar}")
            };
        }
    }
}
