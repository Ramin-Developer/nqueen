using NQueen.GUI.Infrastructure;
using NQueen.GUI.Views;

namespace NQueen.ViewModelTests.Tests.Gui;

[Collection("Serial Test Collection")]
[Trait("Category", "Gui")]
public class GuiServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(typeof(IDispatcher), typeof(WpfDispatcher))]
    [InlineData(typeof(ISaveFileDialogService), typeof(SaveFileDialogService))]
    public void WhenBuiltThenInfrastructureServiceResolvesToWpfImplementation(Type service, Type implementation)
    {
        using var provider = (ServiceProvider)GuiServiceCollectionExtensions.BuildGuiServiceProvider();

        provider.GetRequiredService(service).ShouldBeOfType(implementation);
    }

    [Fact]
    public void WhenBuiltThenMainViewModelResolves()
    {
        using var provider = (ServiceProvider)GuiServiceCollectionExtensions.BuildGuiServiceProvider();

        provider.GetRequiredService<MainViewModel>().ShouldNotBeNull();
    }

    [Fact]
    public void WhenInitializeCalledThenSolverResolves()
    {
        using var provider = (ServiceProvider)GuiServiceCollectionExtensions.Initialize();

        provider.GetRequiredService<ISolver>().ShouldNotBeNull();
    }

    [Theory]
    [InlineData(typeof(MainWindow), ServiceLifetime.Singleton)]
    [InlineData(typeof(MainViewModel), ServiceLifetime.Transient)]
    [InlineData(typeof(ChessboardView), ServiceLifetime.Transient)]
    [InlineData(typeof(InputPanel), ServiceLifetime.Transient)]
    [InlineData(typeof(SimulationPanel), ServiceLifetime.Transient)]
    public void WhenAddedThenViewAndViewModelLifetimesAreRegistered(Type service, ServiceLifetime lifetime)
    {
        var services = new ServiceCollection().AddGuiServices();

        services.Single(d => d.ServiceType == service).Lifetime.ShouldBe(lifetime);
    }
}
