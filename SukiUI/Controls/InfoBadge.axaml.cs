using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using SukiUI.ColorTheme;
using SukiUI.Enums;

namespace SukiUI.Controls;

public class InfoBadge: HeaderedContentControl
{
    private Border? _badgeContainer;
    
    public static readonly StyledProperty<NotificationType> AppearanceProperty =
        AvaloniaProperty.Register<InfoBadge, NotificationType>(nameof(Appearance), NotificationType.Info);

    public NotificationType Appearance
    {
        get => GetValue(AppearanceProperty);
        set
        {
            Background = value switch
            {
                NotificationType.Info => NotificationColor.InfoIconForeground,
                NotificationType.Success => NotificationColor.SuccessIconForeground,
                NotificationType.Warning => NotificationColor.WarningIconForeground,
                NotificationType.Error => NotificationColor.ErrorIconForeground,
                _ => NotificationColor.InfoIconForeground
            };
            
            SetValue(AppearanceProperty, value);
        }
    }

    public static readonly StyledProperty<CornerPosition> CornerPositionProperty = AvaloniaProperty.Register<InfoBadge, CornerPosition>(
        nameof(CornerPosition));
    public CornerPosition CornerPosition
    {
        get => GetValue(CornerPositionProperty);
        set => SetValue(CornerPositionProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsDotProperty = AvaloniaProperty.Register<InfoBadge, bool>(
        nameof(IsDot), false);
    public bool IsDot
    {
        get => GetValue(IsDotProperty);
        set {
            UpdateBadgePosition();
            SetValue(IsDotProperty, value);
        }
    }

    public static readonly StyledProperty<int> OverflowProperty = AvaloniaProperty.Register<InfoBadge, int>(
        nameof(Overflow));
    public int Overflow
    {
        get => GetValue(OverflowProperty);
        set => SetValue(OverflowProperty, value);
    }

    static InfoBadge()
    {
        HeaderProperty.Changed.AddClassHandler<InfoBadge>((badge, _) => badge.UpdateBadgePosition());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _badgeContainer = e.NameScope.Find<Border>("BadgeBorder");
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        UpdateBadgePosition();
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        UpdateBadgePosition();
        return base.ArrangeOverride(finalSize);
    }

    private void UpdateBadgePosition()
    {
        var verticalOffset = -1;
        if (CornerPosition is CornerPosition.BottomLeft or CornerPosition.BottomRight)
        {
            verticalOffset = 1;
        }

        var horizontalOffset = -1;
        if (CornerPosition is CornerPosition.TopRight or CornerPosition.BottomRight)
        {
            horizontalOffset = 1;
        }
        
        if (_badgeContainer is not null && Presenter?.Child is not null)
        {
            _badgeContainer.RenderTransform = new TransformGroup
            {
                Children = new Transforms
                {
                    new TranslateTransform(horizontalOffset*_badgeContainer.Bounds.Width / 2,verticalOffset*_badgeContainer.Bounds.Height / 2)
                }
            };
        }
    }
}