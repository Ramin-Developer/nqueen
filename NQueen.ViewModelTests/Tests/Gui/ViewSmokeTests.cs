using NQueen.GUI.Infrastructure;
using NQueen.GUI.Views;

namespace NQueen.ViewModelTests.Tests.Gui;

[Collection("Serial Test Collection")]
[Trait("Category", "Gui")]
public class ViewSmokeTests
{
    private static bool Hosted<T>(FrameworkElement owner, string name) =>
        owner.FindName(name) is ContentControl { Content: T };

    [Theory]
    [InlineData(typeof(ChessboardView))]
    [InlineData(typeof(InputPanel))]
    [InlineData(typeof(SimulationPanel))]
    [InlineData(typeof(SelectedSolutionBar))]
    [InlineData(typeof(SolutionListPanel))]
    [InlineData(typeof(SolutionSummaryPanel))]
    public void WhenViewConstructedThenComponentInitializes(Type viewType)
    {
        var created = WpfTestHost.Run(() => Activator.CreateInstance(viewType) is UserControl);

        created.ShouldBeTrue();
    }

    [Fact]
    public void WhenInputPanelLoadedThenDelaySliderUsesDomainMinimum()
    {
        var minimum = WpfTestHost.Run(() =>
        {
            var panel = new InputPanel();
            panel.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
            return ((Slider)panel.FindName("DelaySlider")).Minimum;
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
    public void WhenMainWindowHasNullViewModelThenThrows() => WpfTestHost.Run(() =>
                                                                   {
                                                                       using var provider = (ServiceProvider)GuiServiceCollectionExtensions.BuildGuiServiceProvider();
                                                                       Should.Throw<ArgumentNullException>(() => new MainWindow(null!, provider));
                                                                   });

    [Fact]
    public void WhenMainWindowHasNullServiceProviderThenThrows() => WpfTestHost.Run(() =>
                                                                         {
                                                                             using var provider = (ServiceProvider)GuiServiceCollectionExtensions.BuildGuiServiceProvider();
                                                                             var viewModel = provider.GetRequiredService<MainViewModel>();
                                                                             Should.Throw<ArgumentNullException>(() => new MainWindow(viewModel, null!));
                                                                         });
}
