namespace NQueen.GUI.Configuration;

public sealed class TestDispatcher : IDispatcher
{
    public void Invoke(Action action) => action();

    public void BeginInvoke(Action action,
        DispatcherPriority priority = DispatcherPriority.Normal) => action();
}
