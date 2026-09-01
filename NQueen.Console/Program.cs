namespace NQueen.ConsoleApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Non-interactive fast path when flags supplied (skip menu)
        if (args.Length > 0 && ConsoleNonInteractiveRunner.HasSolverArgs(args))
        {
            ConsoleNonInteractiveRunner.Run(args);
            return;
        }

        using var serviceProvider = ConfigureServices();
        var app = serviceProvider.GetRequiredService<App>();
        await app.Run(args); // interactive menu
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Core application registrations
        services.AddNQueenServices(enableCap: true);

        // Root app
        services.AddSingleton<App>();

        return services.BuildServiceProvider();
    }
}
