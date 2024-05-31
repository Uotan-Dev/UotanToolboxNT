using System;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace SukiUI.Controls;

public class GlassCard : ContentControl
{
    public new static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<GlassCard, CornerRadius>(nameof(CornerRadius), new CornerRadius(20));

    public new CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public new static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<GlassCard, Thickness>(nameof(BorderThickness), new Thickness(1));

    public new Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsAnimatedProperty =
        AvaloniaProperty.Register<GlassCard, bool>(nameof(IsAnimated), true);

    public bool IsAnimated
    {
        get => GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsOpaqueProperty =
        AvaloniaProperty.Register<GlassCard, bool>(nameof(IsOpaque), false);

    public bool IsOpaque
    {
        get => GetValue(IsOpaqueProperty);
        set => SetValue(IsOpaqueProperty, value);
    }

    public static readonly StyledProperty<bool> IsInteractiveProperty = AvaloniaProperty.Register<GlassCard, bool>(nameof(IsInteractive));

    public bool IsInteractive
    {
        get => GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    public static readonly StyledProperty<ICommand?> CommandProperty = AvaloniaProperty.Register<GlassCard, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<GlassCard, object?>(nameof(CommandParameter));

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (ContextMenu is null) return;
        ContextMenu.Opening += ContextMenuOnOpening;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (IsAnimated)
        {
            var b = e.NameScope.Get<Panel>("RootPanel");
            b.Loaded += (sender, args) =>
            {
                var v = ElementComposition.GetElementVisual(b);
                MakeOpacityAnimated(v);
            };

            var b2 = e.NameScope.Get<Border>("PART_BorderCard");
            b2.Loaded += (sender, args) =>
            {
                var v = ElementComposition.GetElementVisual(b2);
                MakeSizeAnimated(v);
            };

            var b3 = e.NameScope.Get<Border>("PART_ClipBorder");
            b3.Loaded += (sender, args) =>
            {
                var v = ElementComposition.GetElementVisual(b3);
                MakeSizeAnimated(v);
            };

        }

    }
    

    public static void MakeOpacityAnimated(CompositionVisual compositionVisual)
    {
        if (compositionVisual == null)
            return;

        Compositor compositor = compositionVisual.Compositor;

        var animationGroup = compositor.CreateAnimationGroup();
      
        
        ScalarKeyFrameAnimation opacityAnimation = compositor.CreateScalarKeyFrameAnimation();
        opacityAnimation.Target = "Opacity";
        opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        opacityAnimation.Duration = TimeSpan.FromMilliseconds(700);
        
    
       
        Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(700);
        

        animationGroup.Add(offsetAnimation);
        animationGroup.Add(opacityAnimation);
      
        
        ImplicitAnimationCollection implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
        implicitAnimationCollection["Opacity"] = animationGroup;
        implicitAnimationCollection["Offset"] = animationGroup;

        
        compositionVisual.ImplicitAnimations = implicitAnimationCollection;

    }

    public static void MakeSizeAnimated(CompositionVisual compositionVisual)
    {
        if (compositionVisual == null)
            return;

        Compositor compositor = compositionVisual.Compositor;

        var animationGroup = compositor.CreateAnimationGroup();
        
        Vector2KeyFrameAnimation sizeAnimation = compositor.CreateVector2KeyFrameAnimation();
        sizeAnimation.Target = "Size";
        sizeAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        sizeAnimation.Duration = TimeSpan.FromMilliseconds(450);
        
      
        
        Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(450);
        
        
        
        animationGroup.Add(sizeAnimation);

        animationGroup.Add(offsetAnimation);
        
        ImplicitAnimationCollection implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
        implicitAnimationCollection["Size"] = animationGroup;
        implicitAnimationCollection["Offset"] = animationGroup;

        
        compositionVisual.ImplicitAnimations = implicitAnimationCollection;

    }

    private void ContextMenuOnOpening(object sender, CancelEventArgs e)
    {
        PseudoClasses.Set(":pointerdown", false);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        PseudoClasses.Set(":pointerdown", true);
        if(IsInteractive && Command is not null && Command.CanExecute(CommandParameter))
            Command.Execute(CommandParameter);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        PseudoClasses.Set(":pointerdown", false);
    }
}