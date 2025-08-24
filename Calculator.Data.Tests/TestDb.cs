using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Calculator.Data;

// pomocnicza klasa  tworz¹ca prawdziw¹ baze SQLite in-memory i trzyma otworte po³¹czenie
public sealed class TestDb : IDisposable
{
    public SqliteConnection Connection { get; }
    public AppDbContext Db { get; }

    public TestDb()
    {
        // Jedna pamiêciowa baza na czas ¿ycia TestDb
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(Connection)
            .Options;

        Db = new AppDbContext(options);

        // W testach wystarczy EnsureCreated, szybsze ni¿ migracje
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db.Dispose();
        Connection.Close();
        Connection.Dispose();
    }
}
