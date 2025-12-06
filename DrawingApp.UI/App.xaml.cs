using DrawingApp.Core.Interfaces.Repositories;
using DrawingApp.Core.Interfaces.Services;
using DrawingApp.Data;
using DrawingApp.Data.Repositories;
using DrawingApp.Data.Seed;
using DrawingApp.UI.Navigation;
using DrawingApp.UI.Services;
using DrawingApp.UI.ViewModels;
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
        this.UnhandledException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine("=== UNHANDLED ===");
            System.Diagnostics.Debug.WriteLine(e.Exception?.ToString());
            e.Handled = true;
        };

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<MainWindow>();

                // Db path
                var dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "DrawingApp", "app.db");
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

                services.AddDbContextFactory<AppDbContext>(opt =>
                        opt.UseSqlite($"Data Source={dbPath}"));

                // Repos
                services.AddScoped<IProfileRepository, ProfileRepository>();
                services.AddScoped<IBoardRepository, BoardRepository>();
                services.AddScoped<ITemplateRepository, TemplateRepository>();

                // UI services
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<IThemeService, ThemeService>();
                services.AddSingleton<IProfileSession, ProfileSession>();
                services.AddScoped<IStatisticsService, StatisticsService>();

                // ViewModels
                services.AddTransient<ShellViewModel>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<ProfileViewModel>();
                services.AddTransient<DrawingViewModel>();
                services.AddTransient<BoardsViewModel>();
                services.AddTransient<TemplatesViewModel>();
                services.AddTransient<DashboardViewModel>();
            })
            .Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            using (var scope = Host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await DbSeeder.SeedAsync(db);

            }

            var window = Host.Services.GetRequiredService<MainWindow>();
            window.Activate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("=== APP STARTUP ERROR ===");
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            throw; // để Exception Helper hiện đúng stack
        }
    }
}
