using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using SukiUI.Controls;
using UotanToolbox.Features.Home;

namespace UotanToolbox.Features.Appmgr;

public partial class AppmgrView : UserControl
{
    public AppmgrView()
    {
        InitializeComponent();
        var applicationNames = new string[] { "应用1", "应用2", "应用3" };
        var applicationSizes = new int[] { 100, 200, 300 };
        var applicationInstalled = new DateTime[] { DateTime.Now.AddDays(-1), DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-3) };

        var stackPanel = this.Find<StackPanel>("StackPanel");
        for (int i = 0; i < applicationNames.Length; i++)
        {
            var card = CreateGlassCard(applicationNames[i], applicationSizes[i], applicationInstalled[i]);
            stackPanel.Children.Add(card);
        }
    }

    private Control CreateGlassCard(string appName, int appSize, DateTime appInstalled)
    {
        var card = new GlassCard
        {
            Width = 310
        };

        var border = new Border
        {
            Child = card
        };

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10
        };

        var appNameTextBlock = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 15,
            FontWeight = FontWeight.Bold,
            Text = appName
        };

        var appInfoTextBlock = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 12,
            Text = $"{appSize}MB | {appInstalled:yyyy/M/d}"
        };

        var uninstallTextBlock = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            FontSize = 12,
            Text = "卸载"
        };

        stackPanel.Children.Add(appNameTextBlock);
        stackPanel.Children.Add(appInfoTextBlock);
        stackPanel.Children.Add(new StackPanel { Spacing = 10 });
        stackPanel.Children.Add(uninstallTextBlock);

        card.Content = stackPanel;

        return border;
    }
}