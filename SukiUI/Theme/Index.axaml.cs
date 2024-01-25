using Avalonia;
using Avalonia.Collections;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using DynamicData;
using SukiUI.Controls;
using SukiUI.Enums;
using SukiUI.Extensions;
using SukiUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SukiUI;

public partial class SukiTheme : Styles
{
    public static readonly StyledProperty<SukiColor> ThemeColorProperty =
        AvaloniaProperty.Register<SukiTheme, SukiColor>(nameof(Color), defaultBindingMode: BindingMode.OneTime,
            defaultValue: SukiColor.Blue);

    /// <summary>
    /// Used to assign the ColorTheme at launch,
    /// </summary>
    public SukiColor ThemeColor
    {
        get => GetValue(ThemeColorProperty);
        set
        {
            SetValue(ThemeColorProperty, value);
            SetColorThemeResourcesOnColorThemeChanged();
        }
    }

    /// <summary>
    /// Called whenever the application's <see cref="SukiColorTheme"/> is changed.
    /// Useful where controls cannot use "DynamicResource"
    /// </summary>
    public Action<SukiColorTheme>? OnColorThemeChanged { get; set; }

    /// <summary>
    /// Called whenever the application's <see cref="ThemeVariant"/> is changed.
    /// Useful where controls need to change based on light/dark.
    /// </summary>
    public Action<ThemeVariant>? OnBaseThemeChanged { get; set; }

    /// <summary>
    /// Called whenever the application's Background animation state changes.
    /// Useful where controls need to adapt to the change in background state.
    /// </summary>
    public Action<bool>? OnBackgroundAnimationChanged { get; set; }

    /// <summary>
    /// Currently active <see cref="SukiColorTheme"/>
    /// If you want to change this please use <see cref="ChangeColorTheme(SukiUI.Models.SukiColorTheme)"/>
    /// </summary>
    public SukiColorTheme? ActiveColorTheme { get; private set; }

    /// <summary>
    /// All available Color Themes.
    /// </summary>
    public IAvaloniaReadOnlyList<SukiColorTheme> ColorThemes => _allThemes;

    /// <summary>
    /// Currently active <see cref="ThemeVariant"/>
    /// If you want to change this please use <see cref="ChangeBaseTheme"/> or <see cref="SwitchBaseTheme"/>
    /// </summary>
    public ThemeVariant ActiveBaseTheme => _app.ActualThemeVariant;

    /// <summary>
    /// Tells you if the background is currently animated, if one has been registered.
    /// </summary>
    public bool IsBackgroundAnimated => _background != null && _background.AnimationEnabled;

    private readonly Application _app;

    private readonly HashSet<SukiColorTheme> _colorThemeHashset = new();
    private readonly AvaloniaList<SukiColorTheme> _allThemes = new();

    private SukiBackground? _background;

    public SukiTheme()
    {
        AvaloniaXamlLoader.Load(this);
        _app = Application.Current!;
        _app.ActualThemeVariantChanged += (_, e) => OnBaseThemeChanged?.Invoke(_app.ActualThemeVariant);
        foreach (var theme in DefaultColorThemes)
            AddColorTheme(theme.Value);
    }

    /// <summary>
    /// Change the theme to one of the default themes.
    /// </summary>
    /// <param name="sukiColor">The <see cref="SukiColor"/> to change to.</param>
    public void ChangeColorTheme(SukiColor sukiColor) =>
        ThemeColor = sukiColor;

    /// <summary>
    /// Tries to change the theme to a specific theme, this can be either a default or a custom defined one.
    /// </summary>
    /// <param name="sukiColorTheme"></param>
    public void ChangeColorTheme(SukiColorTheme sukiColorTheme) =>
        SetColorTheme(sukiColorTheme);

    /// <summary>
    /// Blindly switches to the "next" theme available in the <see cref="ColorThemes"/> collection.
    /// </summary>
    public void SwitchColorTheme()
    {
        var index = ColorThemes.IndexOf(ActiveColorTheme);
        if (index == -1) return;
        var newIndex = (index + 1) % ColorThemes.Count;
        var newColorTheme = ColorThemes[newIndex];
        ChangeColorTheme(newColorTheme);
    }

    /// <summary>
    /// Add a new <see cref="SukiColorTheme"/> to the ones available, without making it active.
    /// </summary>
    /// <param name="sukiColorTheme">New <see cref="SukiColorTheme"/> to add.</param>
    public void AddColorTheme(SukiColorTheme sukiColorTheme)
    {
        if (_colorThemeHashset.Contains(sukiColorTheme))
            throw new InvalidOperationException("This color theme has already been added.");
        _colorThemeHashset.Add(sukiColorTheme);
        _allThemes.Add(sukiColorTheme);
    }

    /// <summary>
    /// Adds multiple new <see cref="SukiColorTheme"/> to the ones available, without making any active.
    /// </summary>
    /// <param name="sukiColorThemes">A collection of new <see cref="SukiColorTheme"/> to add.</param>
    public void AddColorThemes(IEnumerable<SukiColorTheme> sukiColorThemes)
    {
        foreach (var colorTheme in sukiColorThemes)
            AddColorTheme(colorTheme);
    }

    /// <summary>
    /// Tries to change the base theme to the one provided, if it is different.
    /// </summary>
    /// <param name="baseTheme"><see cref="ThemeVariant"/> to change to.</param>
    public void ChangeBaseTheme(ThemeVariant baseTheme)
    {
        if (_app.ActualThemeVariant == baseTheme) return;
        _app.RequestedThemeVariant = baseTheme;
    }

    /// <summary>
    /// Simply switches from Light -> Dark and visa versa.
    /// </summary>
    public void SwitchBaseTheme()
    {
        if (Application.Current is null) return;
        var newBase = Application.Current.ActualThemeVariant == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
        Application.Current.RequestedThemeVariant = newBase;
    }

    /// <summary>
    /// Attempts to switch the currently active background animation state to a specific value.
    /// </summary>
    /// <param name="value"></param>
    public void SetBackgroundAnimationsEnabled(bool value) =>
        _background?.SetAnimationEnabled(value);

    /// <summary>
    /// Attempts to switch the currently active background animation state from whatever it is, to the opposite.
    /// </summary>
    public void SwitchBackgroundAnimationsEnabled() =>
        _background?.SetAnimationEnabled(_background.AnimationEnabled);

    /// <summary>
    /// Registers a background with the instance, if one hasn't already been registered.
    /// </summary>
    internal void RegisterBackground(SukiBackground background) =>
        _background ??= background;

    /// <summary>
    /// Initializes the color theme resources whenever the property is changed.
    /// In an ideal world people wouldn't use the property
    /// </summary>
    private void SetColorThemeResourcesOnColorThemeChanged()
    {
        if (!DefaultColorThemes.TryGetValue(ThemeColor, out var colorTheme))
            throw new Exception($"{ThemeColor} has no defined color theme.");
        SetColorTheme(colorTheme);
    }

    private void SetColorTheme(SukiColorTheme colorTheme)
    {
        SetColorWithOpacities("SukiPrimaryColor", colorTheme.Primary);
        SetColorWithOpacities("SukiAccentColor", colorTheme.Accent);
        ActiveColorTheme = colorTheme;
        OnColorThemeChanged?.Invoke(ActiveColorTheme);
    }

    private void SetColorWithOpacities(string baseName, Color baseColor)
    {
        SetResource(baseName, baseColor);
        SetResource($"{baseName}75", baseColor.WithAlpha(0.75));
        SetResource($"{baseName}50", baseColor.WithAlpha(0.50));
        SetResource($"{baseName}25", baseColor.WithAlpha(0.25));
        SetResource($"{baseName}15", baseColor.WithAlpha(0.15));
        SetResource($"{baseName}10", baseColor.WithAlpha(0.10));
        SetResource($"{baseName}5", baseColor.WithAlpha(0.05));
        SetResource($"{baseName}0", baseColor.WithAlpha(0.00));
    }

    private void SetResource(string name, Color color) =>
        _app.Resources[name] = color;

    // Static Members...

    /// <summary>
    /// The default Color Themes included with SukiUI.
    /// </summary>
    public static readonly IReadOnlyDictionary<SukiColor, SukiColorTheme> DefaultColorThemes;

    static SukiTheme()
    {
        var defaultThemes = new[]
        {
            new DefaultSukiColorTheme(SukiColor.Orange, Color.Parse("#ED8E12"), Color.Parse("#176CE8")),
            new DefaultSukiColorTheme(SukiColor.Red, Color.Parse("#D03A2F"), Color.Parse("#2FC5D0")),
            new DefaultSukiColorTheme(SukiColor.Green, Colors.ForestGreen, Color.Parse("#B24DB0")),
            new DefaultSukiColorTheme(SukiColor.Blue, Color.Parse("#0A59F7"), Color.Parse("#F7A80A"))
        };
        DefaultColorThemes = defaultThemes.ToDictionary(x => x.ThemeColor, y => (SukiColorTheme)y);
    }

    /// <summary>
    /// Retrieves an instance tied to a specific instance of an application.
    /// </summary>
    /// <returns>A <see cref="SukiTheme"/> instance that can be used to change themes.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no SukiTheme has been defined in App.axaml.</exception>
    public static SukiTheme GetInstance(Application app)
    {
        var theme = app.Styles.FirstOrDefault(style => style is SukiTheme);
        if (theme is not SukiTheme sukiTheme)
            throw new InvalidOperationException(
                "No SukiTheme instance available. Ensure SukiTheme has been set in Application.Styles in App.axaml.");
        return sukiTheme;
    }

    /// <summary>
    /// Retrieves an instance tied to the currently active application.
    /// </summary>
    /// <returns>A <see cref="SukiTheme"/> instance that can be used to change themes.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no SukiTheme has been defined in App.axaml.</exception>
    public static SukiTheme GetInstance() => GetInstance(Application.Current!);
}