using System;
using Xunit;
using Calculator.Domain.Services;
using Calculator.Domain.Exceptions;

namespace Calculator.Domain.Tests;

public class BasicCalculatorEngineTests
{
    private readonly BasicCalculatorEngine _engine = new();

    [Theory]
    [InlineData("3+5", 8)]
    [InlineData("10-2", 8)]
    [InlineData("2*3", 6)]
    [InlineData("8/2", 4)]
    [InlineData("0/5", 0)]
    [InlineData("0*5", 0)]
    [InlineData("-3*4", -12)]                        // minut przed a
    [InlineData("3*-4", -12)]
    [InlineData("-3*-4", 12)]
    [InlineData("+3+5", 8)]                          // plus przed a
    [InlineData("  3   +          5  ", 8)]          // białe znaki
    [InlineData("3,5+2", 5.5)]                       // przecinek
    [InlineData("3.5+2", 5.5)]                       // kropka
    [InlineData("\t3\n+\r5", 8)]                     // taby/entery
    [InlineData("3+-5", -2)]
    [InlineData("3--5", 8)]                          // odejmowanie minusa
    [InlineData("3++5", 8)]                          // dodawnia plusów
    [InlineData("3*-5", -15)]
    [InlineData("3/-5", -0.6)]
    [InlineData("-0+5", 5)]                          // ujemne zero
    [InlineData("0.0001+0.0002", 0.0003)]            // operacje na przecikach
    [InlineData("999999999999+1", 1000000000000)]
    [InlineData("1,000+2", 3)]                       // dodawanie ulamu do całości
    [InlineData("3-0.003", 2.997)]                   // odejmowanie od całoci ulamku
    [InlineData("3.005-2", 1.005)]                   // ułamek - całe
    [InlineData("5 6+4", 60)]                        // łączenie liczb przy przypadkowej spacji
    [InlineData("1. 000+2", 3)]
    [InlineData("1 .000+2", 3)]
    [InlineData("-5+6+" , 1)]                        // tu teoretycznie mozna zrobić walidacje  i nie przepuszczac tego
    [InlineData("3.+2" , 5)]                         // tak samo tutaj, ale to jak czasu starczy
    public void Evaluate_ValidSimpleExpressions_ReturnsExpected(string expr, decimal expected)
    {
        var result = _engine.Evaluate(expr);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Evaluate_DivisionByZero_ThrowsDomainException()
    {
        Assert.Throws<DivisionByZeroDomainException>(() => _engine.Evaluate("3/0"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("+")]
    [InlineData("-")]
    [InlineData("a")]
    [InlineData("3+")]
    [InlineData("+5")]
    [InlineData("a+b")]
    [InlineData("a+5")]
    [InlineData("5+b")]
    [InlineData("-5+++6")]
    [InlineData("--5+6")]
    [InlineData("3 + 5 + 6")]
    [InlineData("3..5+2")]
    [InlineData("3+f5")]
    [InlineData("3^2")]
    [InlineData("1.2.3+4")]
    public void Evaluate_InvalidExpressions_ThrowInvalidExpression(string expr)
    {
        Assert.Throws<DomainInvalidExpressionException>(() => _engine.Evaluate(expr));
    }
}
