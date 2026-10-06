namespace NQueen.ViewModelTests.Tests.Gui;

[Collection("Serial Test Collection")]
[Trait("Category", "Gui")]
public class WpfDispatcherTests
{
    [Fact]
    public void WhenInvokedFromWorkerThenActionRunsOnUiThread()
    {
        var uiThread = WpfTestHost.Dispatcher.Thread;
        Thread? ranOn = null;

        new WpfDispatcher().Invoke(() => ranOn = Thread.CurrentThread);

        ranOn.ShouldBeSameAs(uiThread);
    }

    [Fact]
    public void WhenBeginInvokedFromWorkerThenActionRunsOnUiThread()
    {
        var uiThread = WpfTestHost.Dispatcher.Thread;
        Thread? ranOn = null;
        using var done = new ManualResetEventSlim();

        new WpfDispatcher().BeginInvoke(() =>
        {
            ranOn = Thread.CurrentThread;
            done.Set();
        }, DispatcherPriority.Background);

        done.Wait(TimeSpan.FromSeconds(10)).ShouldBeTrue();
        ranOn.ShouldBeSameAs(uiThread);
    }
}
