using System.Windows;
using Calculator.Data;
using Calculator.Data.Services;
using Calculator.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Calculator.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Konfiguracja
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        // DI
        var sc = new ServiceCollection();
        sc.AddLogging(b => b.AddDebug());

        var cs = config.GetConnectionString("Default") ?? "Data Source=calculator.db";
        sc.AddDbContext<AppDbContext>(opt => opt.UseSqlite(cs));

        // Domain
        sc.AddSingleton<ICalculatorEngine, BasicCalculatorEngine>();

        // Data (impl. interfejsów domeny)
        sc.AddScoped<IHistoryService, EfHistoryService>();
        sc.AddTransient<HistoryWindow>();

        // WPF
        sc.AddSingleton<IConfiguration>(config);
        sc.AddTransient<MainWindow>();

        Services = sc.BuildServiceProvider();

        // Auto-migracja
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        // Start
        var main = Services.GetRequiredService<MainWindow>();
        MainWindow = main;
        main.Show();

        base.OnStartup(e);
    }
}
