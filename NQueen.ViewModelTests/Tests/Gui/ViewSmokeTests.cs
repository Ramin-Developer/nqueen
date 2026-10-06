using NQueen.GUI.Infrastructure;
using NQueen.GUI.Views;

namespace NQueen.ViewModelTests.Tests.Gui;

[Collection("Serial Test Collection")]
[Trait("Category", "Gui")]
public class ViewSmokeTests
{
    private static bool Hosted<T>(FrameworkElement owner, string name) =>
        owner.FindName(name) is System.Windows.Controls.ContentControl { Content: T };

    [Theory]
    [InlineData(typeof(ChessboardView))]
    [InlineData(typeof(InputPanel))]
    [InlineData(typeof(SimulationPanel))]
    [InlineData(typeof(SelectedSolutionBar))]
    [InlineData(typeof(SolutionListPanel))]
    [InlineData(typeof(SolutionSummaryPanel))]
    public void WhenViewConstructedThenComponentInitializes(Type viewType)
    {
        var created = WpfTestHost.Run(() => Activator.CreateInstance(viewType) is System.Windows.Controls.UserControl);

        created.ShouldBeTrue();
    }

    [Fact]
    public void WhenInputPanelLoadedThenDelaySliderUsesDomainMinimum()
    {
        var minimum = WpfTestHost.Run(() =>
        {
            var panel = new InputPanel();
            panel.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            return ((System.Windows.Controls.Slider)panel.FindName("DelaySlider")).Minimum;
        });

        minimum.ShouldBe(SimulationSettings.MinDelayInMilliseconds);
    }

    [Fact]
    public void WhenMainWindowConstructedThenPlaceholdersHostChildViews()
    {
        var hosted = WpfTestHost.Run(() =>
        {
            using var provider = (ServiceProvider)GuiServiceCollectionExtensions.BuildGuiServiceProvider();
            using var window = provider.GetRequiredService<MainWindow>();
            return Hosted<ChessboardView>(window, "chessboardPlaceholder")
                && Hosted<InputPanel>(window, "inputPanelPlaceHolder")
                && Hosted<SimulationPanel>(window, "simulationPanelPlaceHolder");
        });

        hosted.ShouldBeTrue();
    }

    [Fact]
    public void WhenMainWindowLoadedThenBoardUsesDesignSize()
    {
        var width = WpfTestHost.Run(() =>
        {
            using var provider = (ServiceProvider)GuiServiceCollectionExtensions.BuildGuiServiceProvider();
            using var window = provider.GetRequiredService<MainWindow>();
            window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            return window.MainViewModel.ChessboardVm.WindowWidth;
        });

        width.ShouldBe(640);
    }

    [Fact]
    public void WhenMainWindowDisposedTwiceThenViewModelIsReleased()
    {
        var released = WpfTestHost.Run(() =>
        {
            using var provider = (ServiceProvider)GuiServiceCollectionExtensions.BuildGuiServiceProvider();
            var window = provider.GetRequiredService<MainWindow>();
            window.Dispose();
            window.Dispose();
            return window.MainViewModel is null;
        });

        released.ShouldBeTrue();
    }

    [Fact]
    public void WhenMainWindowHasNullViewModelThenThrows()
    {
        var thrown = WpfTestHost.Run(() =>
        {
            using var provider = (ServiceProvider)GuiServiceCollectionExtensions.BuildGuiServiceProvider();
            try
            {
                _ = new MainWindow(null!, provider);
                return false;
            }
            catch (ArgumentNullException)
            {
                return true;
            }
        });

        thrown.ShouldBeTrue();
    }

    [Fact]
    public void WhenMainWindowHasNullServiceProviderThenThrows()
    {
        var thrown = WpfTestHost.Run(() =>
        {
            using var provider = (ServiceProvider)GuiServiceCollectionExtensions.BuildGuiServiceProvider();
            try
            {
                _ = new MainWindow(provider.GetRequiredService<MainViewModel>(), null!);
                return false;
            }
            catch (ArgumentNullException)
            {
                return true;
            }
        });

        thrown.ShouldBeTrue();
    }
}
