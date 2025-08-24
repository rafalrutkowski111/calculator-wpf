using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Calculator.Data.Services;
using Calculator.Domain.Services;
using FluentAssertions;
using Xunit;

public class EfHistoryServiceTests
{

    private static DateTime ToUtcLocal(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Local));

    //sprawdzanie czy filtruje dokłądnie 500 elementów i czy są poprawnie posortowane
    [Fact]
    public async Task Save_and_GetLast_limit_works()
    {
        using var tdb = new TestDb();
        IHistoryService svc = new EfHistoryService(tdb.Db);

        for (int i = 1; i <= 600; i++)
            await svc.SaveAsync($"{i}+1", (i + 1).ToString(CultureInfo.InvariantCulture), DateTime.UtcNow.AddMinutes(i));

        var last500 = await svc.GetLastAsync(500);

        last500.Should().HaveCount(500);
        last500.First().Expression.Should().Be("600+1");
        last500.Last().Expression.Should().Be("101+1");
    }

    // sprawdzanie czy serch działa na Expression i Result
    [Fact]
    public async Task Search_by_expression_and_result_returns_last_500_matching()
    {
        using var tdb = new TestDb();
        IHistoryService svc = new EfHistoryService(tdb.Db);

        await svc.SaveAsync("2+2", "4", DateTime.UtcNow);
        await svc.SaveAsync("123+1", "124", DateTime.UtcNow);
        await svc.SaveAsync("9*9", "81", DateTime.UtcNow);

        var byExpr = await svc.SearchAsync("123", 500);
        byExpr.Should().ContainSingle(x => x.Expression == "123+1");

        var byRes = await svc.SearchAsync("81", 500);
        byRes.Should().ContainSingle(x => x.Result == "81");
    }

    // sprawdzanie czy search działa porpawnie na konkretnej dacie lokalnej yyyy-MM-dd
    [Fact]
    public async Task Search_by_local_date_with_strftime_localtime_matches()
    {
        using var tdb = new TestDb();
        IHistoryService svc = new EfHistoryService(tdb.Db);

        // wstaw dwa wpisy w różnych lokalnych dniach
        var todayLocal = DateTime.Now;
        var otherLocal = todayLocal.AddDays(-2);

        await svc.SaveAsync("A", "1", ToUtcLocal(todayLocal));
        await svc.SaveAsync("B", "2", ToUtcLocal(otherLocal));

        var key = todayLocal.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var rows = await svc.SearchAsync(key, 500);

        rows.Should().ContainSingle(x => x.Expression == "A");
        rows.Should().NotContain(x => x.Expression == "B");
    }


    // sprawdzanie czy search działa porpawnie na konkretnej dacie lokalnej yyyy-MM
    [Fact]
    public async Task Search_by_month_localtime_matches()
    {
        using var tdb = new TestDb();
        IHistoryService svc = new EfHistoryService(tdb.Db);

        var d1 = new DateTime(2025, 8, 5, 10, 0, 0);
        var d2 = new DateTime(2025, 8, 24, 22, 0, 0);
        var d3 = new DateTime(2025, 9, 1, 12, 0, 0);

        await svc.SaveAsync("M8a", "x", ToUtcLocal(d1));
        await svc.SaveAsync("M8b", "y", ToUtcLocal(d2));
        await svc.SaveAsync("M9", "z", ToUtcLocal(d3));

        var rows = await svc.SearchAsync("2025-08", 500);
        rows.Select(r => r.Expression).Should().BeEquivalentTo(new[] { "M8b", "M8a" });
    }

    //brak wyników
    [Fact]
    public async Task Search_with_no_matches_returns_empty_list()
    {
        using var tdb = new TestDb();
        IHistoryService svc = new EfHistoryService(tdb.Db);

        await svc.SaveAsync("2+2", "4", DateTime.UtcNow);
        await svc.SaveAsync("3+3", "6", DateTime.UtcNow);

        var rows = await svc.SearchAsync("xyz", 500);

        rows.Should().BeEmpty();
    }

    // testujemy filtrowanie bez nałozonego filtra
    [Fact]
    public async Task Search_with_empty_query_returns_last_limit()
    {
        using var tdb = new TestDb();
        IHistoryService svc = new EfHistoryService(tdb.Db);

        for (int i = 1; i <= 10; i++)
            await svc.SaveAsync($"{i}+1", (i + 1).ToString(), DateTime.UtcNow);

        var rows = await svc.SearchAsync("", 5);

        rows.Should().HaveCount(5);
        rows.First().Expression.Should().Be("10+1");
        rows.Last().Expression.Should().Be("6+1");
    }

}
