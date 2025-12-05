using DrawingApp.Core.Interfaces.Repositories;
using DrawingApp.Core.Interfaces.Services;
using DrawingApp.Data;
using DrawingApp.Data.Repositories;
using DrawingApp.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using System;
using System.IO;

namespace DrawingApp.UI;

public partial class App : Application
{
    public static IHost Host { get; private set; } = null!;

    public App()
    {
        this.InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                // Db path
                var dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "DrawingApp", "app.db");
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

                services.AddDbContext<AppDbContext>(opt =>
                    opt.UseSqlite($"Data Source={dbPath}"));

                // Repos
                services.AddScoped<IProfileRepository, ProfileRepository>();
                services.AddScoped<IBoardRepository, BoardRepository>();
                services.AddScoped<ITemplateRepository, TemplateRepository>();
            })
            .Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        using (var scope = Host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DbSeeder.SeedAsync(db);
        }

        var window = Host.Services.GetRequiredService<MainWindow>();
        window.Activate();
    }
}
