using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SukiUI.Controls;

public class SettingsLayoutItem
{
    public string Header { get; set; }
    public Control Content { get; set; }
}

public partial class SettingsLayout : UserControl
{
    public SettingsLayout()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateItems();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private ObservableCollection<SettingsLayoutItem> _items;

    public static readonly DirectProperty<SettingsLayout, ObservableCollection<SettingsLayoutItem>> StepsProperty =
        AvaloniaProperty.RegisterDirect<SettingsLayout, ObservableCollection<SettingsLayoutItem>>(nameof(Items), l => l.Items,
            (numpicker, v) =>
            {
                numpicker.Items = v;
            }, defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public ObservableCollection<SettingsLayoutItem> Items
    {
        get { return _items; }
        set { SetAndRaise(StepsProperty, ref _items, value); }
    }

    private void UpdateItems()
    {
        if (Items is null)
        {
            return;
        }

        var stackSummary = (StackPanel)this.GetTemplateChildren().First(n => n.Name == "StackSummary");
        var myScroll = (ScrollViewer)this.GetTemplateChildren().First(n => n.Name == "MyScroll");

        if (myScroll.Content is not StackPanel stackItems)
            return;

        stackItems.Children.Clear();
        stackSummary.Children.Clear();

        var radios = new List<RadioButton>();
        var borders = new List<Border>();

        stackItems.Children.Add(new Border() { Height = 8 });

        foreach (var settingsLayoutItem in Items)
        {
            var border = new Border
            {
                Child = new GroupBox()
                {
                    Margin = new Thickness(10, 20),
                    Header = new TextBlock() { Text = settingsLayoutItem.Header },
                    Content = new Border()
                    {
                        Margin = new Thickness(35, 12),
                        Child = settingsLayoutItem.Content
                    }
                }
            };

            borders.Add(border);
            stackItems.Children.Add(border);

            var summaryButton = new RadioButton()
            {
                Content = new TextBlock() { Text = settingsLayoutItem.Header, FontSize = 17 },
                Classes = { new string[] { "MenuChip" } }
            };
            summaryButton.Click += async (sender, args) =>
            {
                if (isAnimatingScroll)
                    return;
                var x = border.TranslatePoint(new Point(), stackItems);

                if (x.HasValue)
                    await AnimateScroll(x.Value.Y);  // myScroll.Offset = new Vector(0, x.Value.Y);
            };
            radios.Add(summaryButton);
            stackSummary.Children.Add(summaryButton);
        }

        stackSummary.Children.Add(new Border() { Height = 300 });

        myScroll.ScrollChanged += (sender, args) =>
        {
            if (isAnimatingScroll)
                return;

            var OffsetY = myScroll.Offset.Y;

            var l = borders.Select(b => Math.Abs(b.TranslatePoint(new Point(), stackItems).Value.Y - OffsetY)).ToList();
            radios[l.IndexOf(l.Min())].IsChecked = true;
        };
    }

    private async void DockPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (isAnimatingWidth)
            return;

        var currentwidth = this.GetTemplateChildren().First(n => n.Name == "StackSummary").Width;
        var desiredSize = e.NewSize.Width > 1000 ? 400 : 0;

        if (desiredSize != currentwidth)
            if ((currentwidth == 0 || currentwidth == 400))
                await AnimateSummaryWidth(this.GetTemplateChildren().First(n => n.Name == "StackSummary").Width, desiredSize);

        if (e.NewSize.Width <= 1000 && e.NewSize.Width > 850)
            await AnimateItemsMargin(new Thickness(100, 0));
        else if (e.NewSize.Width <= 850 && e.NewSize.Width > 700)
            await AnimateItemsMargin(new Thickness(50, 0));
        else
            await AnimateItemsMargin(new Thickness(-10, 0));
    }

    private bool isAnimatingWidth = false;
    private bool isAnimatingMargin = false;
    private bool isAnimatingScroll = false;

    private async Task AnimateItemsMargin(Thickness desiredSize)
    {
        if (isAnimatingMargin)
            return;

        var myScroll = (ScrollViewer)this.GetTemplateChildren().First(n => n.Name == "MyScroll");
        if (myScroll.Content is not StackPanel stackItems)
            return;

        if (stackItems.Margin.Left == desiredSize.Left)
            return;

        isAnimatingMargin = true;

        var animationTask = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(800),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame()
                {
                    Setters = { new Setter { Property = MarginProperty, Value = stackItems.Margin } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters = { new Setter { Property = MarginProperty, Value = desiredSize } },
                    KeyTime = TimeSpan.FromMilliseconds(800)
                }
            }
        }.RunAsync(stackItems);

        var abortTask = Task.Run(async () =>
        {
            await Task.Delay(1000);
            isAnimatingMargin = false;
        });

        await Task.WhenAll(animationTask, abortTask);
    }

    private async Task AnimateSummaryWidth(double current, double desiredSize)
    {
        isAnimatingWidth = true;

        var animationTask = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(800),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame()
                {
                    Setters = { new Setter { Property = WidthProperty, Value = current } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters = { new Setter { Property = WidthProperty, Value = desiredSize } },
                    KeyTime = TimeSpan.FromMilliseconds(800)
                }
            }
        }.RunAsync(this.GetTemplateChildren().First(n => n.Name == "StackSummary"));

        var abortTask = Task.Run(async () =>
        {
            await Task.Delay(1000);
            isAnimatingWidth = false;
        });

        await Task.WhenAll(animationTask, abortTask);
    }

    private async Task AnimateScroll(double desiredScroll)
    {
        isAnimatingScroll = true;
        var myscroll = (ScrollViewer)this.GetTemplateChildren().First(n => n.Name == "MyScroll");

        var animationTask = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(800),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame()
                {
                    Setters = { new Setter { Property = ScrollViewer.OffsetProperty, Value = myscroll.Offset } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame()
                {
                    Setters = { new Setter { Property = ScrollViewer.OffsetProperty, Value = new Vector(myscroll.Offset.X, desiredScroll) } },
                    KeyTime = TimeSpan.FromMilliseconds(800)
                }
            }
        }.RunAsync(myscroll);

        var abortTask = Task.Run(async () =>
        {
            await Task.Delay(1000);
            isAnimatingScroll = false;
        });

        await Task.WhenAll(animationTask, abortTask);
    }
}