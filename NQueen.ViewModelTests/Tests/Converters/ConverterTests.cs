using NQueen.GUI.Converters;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NQueen.ViewModelTests.Tests.Converters;

/// <summary>
/// Coverage for the GUI IValueConverter implementations. All three are pure and
/// deterministic: DisplayModeToEnabledConverter (mode match -> bool),
/// NullImageConverter (null/blank -> UnsetValue, else pass-through), and
/// StringNotEmptyToVisibilityConverter (non-blank -> Visible, else Collapsed).
/// </summary>
[Trait("Category", "Converters")]
public class ConverterTests
{
    private static readonly CultureInfo s_culture = CultureInfo.InvariantCulture;

    // ── DisplayModeToEnabledConverter ─────────────────────────────────────────

    [Fact]
    public void DisplayModeToEnabled_MatchingMode_ReturnsTrue()
    {
        var converter = new DisplayModeToEnabledConverter();
        var result = converter.Convert(DisplayMode.Visualize, typeof(bool), "Visualize", s_culture);
        result.ShouldBe(true);
    }

    [Fact]
    public void DisplayModeToEnabled_NonMatchingMode_ReturnsFalse()
    {
        var converter = new DisplayModeToEnabledConverter();
        var result = converter.Convert(DisplayMode.Hide, typeof(bool), "Visualize", s_culture);
        result.ShouldBe(false);
    }

    [Fact]
    public void DisplayModeToEnabled_UnparsableParameter_ReturnsFalse()
    {
        var converter = new DisplayModeToEnabledConverter();
        var result = converter.Convert(DisplayMode.Visualize, typeof(bool), "NotAMode", s_culture);
        result.ShouldBe(false);
    }

    [Fact]
    public void DisplayModeToEnabled_NonDisplayModeValue_ReturnsFalse()
    {
        var converter = new DisplayModeToEnabledConverter();
        var result = converter.Convert("not-a-mode", typeof(bool), "Visualize", s_culture);
        result.ShouldBe(false);
    }

    [Fact]
    public void DisplayModeToEnabled_ConvertBack_Throws()
    {
        var converter = new DisplayModeToEnabledConverter();
        Should.Throw<NotImplementedException>(() =>
            converter.ConvertBack(true, typeof(DisplayMode), "Visualize", s_culture));
    }

    // ── NullImageConverter ────────────────────────────────────────────────────

    [Fact]
    public void NullImage_NullValue_ReturnsUnsetValue()
    {
        var converter = new NullImageConverter();
        converter.Convert(null!, typeof(object), null!, s_culture)
            .ShouldBe(DependencyProperty.UnsetValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NullImage_BlankString_ReturnsUnsetValue(string input)
    {
        var converter = new NullImageConverter();
        converter.Convert(input, typeof(object), null!, s_culture)
            .ShouldBe(DependencyProperty.UnsetValue);
    }

    [Fact]
    public void NullImage_NonBlankValue_PassesThrough()
    {
        var converter = new NullImageConverter();
        const string path = "some/image.png";
        converter.Convert(path, typeof(object), null!, s_culture).ShouldBe(path);
    }

    [Fact]
    public void NullImage_ConvertBack_ReturnsBindingDoNothing()
    {
        var converter = new NullImageConverter();
        converter.ConvertBack("x", typeof(object), null!, s_culture)
            .ShouldBe(Binding.DoNothing);
    }

    // ── StringNotEmptyToVisibilityConverter ───────────────────────────────────

    [Fact]
    public void StringNotEmpty_NonBlank_ReturnsVisible()
    {
        var converter = new StringNotEmptyToVisibilityConverter();
        converter.Convert("hello", typeof(Visibility), null!, s_culture)
            .ShouldBe(Visibility.Visible);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void StringNotEmpty_BlankOrNull_ReturnsCollapsed(string? input)
    {
        var converter = new StringNotEmptyToVisibilityConverter();
        converter.Convert(input!, typeof(Visibility), null!, s_culture)
            .ShouldBe(Visibility.Collapsed);
    }

    [Fact]
    public void StringNotEmpty_ConvertBack_Throws()
    {
        var converter = new StringNotEmptyToVisibilityConverter();
        Should.Throw<NotImplementedException>(() =>
            converter.ConvertBack(Visibility.Visible, typeof(string), null!, s_culture));
    }
}
