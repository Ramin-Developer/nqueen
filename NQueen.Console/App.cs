namespace NQueen.ConsoleApp;

public class App(IServiceProvider serviceProvider)
{
    public Task Run()
    {
        DispatchCommands.RunInteractiveMenu(serviceProvider);
        return Task.CompletedTask;
    }
}
