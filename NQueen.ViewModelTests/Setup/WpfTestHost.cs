namespace NQueen.ViewModelTests.Setup;

/// <summary>
/// Hosts a single WPF <see cref="NQueen.GUI.App"/> on a dedicated STA thread with a running
/// dispatcher, so views and WPF-bound services can be exercised without showing windows.
/// </summary>
public static class WpfTestHost
{
    private static readonly Lazy<Dispatcher> _dispatcher = new(Start, LazyThreadSafetyMode.ExecutionAndPublication);

    public static Dispatcher Dispatcher => _dispatcher.Value;

    public static void Run(Action action) => Dispatcher.Invoke(action);

    public static T Run<T>(Func<T> func) => Dispatcher.Invoke(func);

    private static Dispatcher Start()
    {
        Dispatcher? dispatcher = null;
        using var ready = new ManualResetEventSlim();
        var thread = new Thread(() =>
        {
            var app = new NQueen.GUI.App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            app.InitializeComponent();
            dispatcher = Dispatcher.CurrentDispatcher;
            ready.Set();
            Dispatcher.Run();
        })
        {
            IsBackground = true,
            Name = "WpfTestHost",
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        ready.Wait();
        return dispatcher!;
    }
}
