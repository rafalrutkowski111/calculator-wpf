using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Calculator.Data;

// pomocnicza klasa  tworz�ca prawdziw� baze SQLite in-memory i trzyma otworte po��czenie
public sealed class TestDb : IDisposable
{
    public SqliteConnection Connection { get; }
    public AppDbContext Db { get; }

    public TestDb()
    {
        // Jedna pami�ciowa baza na czas �ycia TestDb
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(Connection)
            .Options;

        Db = new AppDbContext(options);

        // W testach wystarczy EnsureCreated, szybsze ni� migracje
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db.Dispose();
        Connection.Close();
        Connection.Dispose();
    }
}
