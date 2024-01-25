using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace SukiUI.Theme
{

    public static class Scrollable
    {
        public static void MakeScrollable(CompositionVisual compositionVisual)
        {
            if (compositionVisual == null)
                return;
        
            Compositor compositor = compositionVisual.Compositor;

            var animationGroup = compositor.CreateAnimationGroup();
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = "Offset";

            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(250);

            ImplicitAnimationCollection implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
            animationGroup.Add(offsetAnimation);
            implicitAnimationCollection["Offset"] = animationGroup;
            compositionVisual.ImplicitAnimations = implicitAnimationCollection;
        }
    }
    public static class StackPanelExtensions
    {
        public static readonly AttachedProperty<bool> AnimatedScrollProperty =
            AvaloniaProperty.RegisterAttached<StackPanel, bool>("AnimatedScroll", typeof(StackPanel), defaultValue: false);

        static StackPanelExtensions()
        {
            AnimatedScrollProperty.Changed.AddClassHandler<StackPanel>(HandleAnimatedScrollChanged);
        }
        
        private static void HandleAnimatedScrollChanged(StackPanel interactElem, AvaloniaPropertyChangedEventArgs args)
        {
            if(GetAnimatedScroll(interactElem))
                interactElem.AttachedToVisualTree += (sender, args) => Scrollable.MakeScrollable(ElementComposition.GetElementVisual(interactElem));
        }

        public static bool GetAnimatedScroll(StackPanel wrap)
        {
            return wrap.GetValue(AnimatedScrollProperty);
        }

        public static void SetAnimatedScroll(StackPanel wrap, bool value)
        {


            wrap.SetValue(AnimatedScrollProperty, value);
        }

    }
    
    public static class WrapPanelExtensions
    {
        public static readonly AttachedProperty<bool> AnimatedScrollProperty =
            AvaloniaProperty.RegisterAttached<WrapPanel, bool>("AnimatedScroll", typeof(WrapPanel), defaultValue: false);

        static WrapPanelExtensions()
        {
            AnimatedScrollProperty.Changed.AddClassHandler<WrapPanel>(HandleAnimatedScrollChanged);
        }
        
        private static void HandleAnimatedScrollChanged(WrapPanel interactElem, AvaloniaPropertyChangedEventArgs args)
        {
            if(GetAnimatedScroll(interactElem))
                interactElem.AttachedToVisualTree += (sender, args) => Scrollable.MakeScrollable(ElementComposition.GetElementVisual(interactElem));
        }

        public static bool GetAnimatedScroll(WrapPanel wrap)
        {
            return wrap.GetValue(AnimatedScrollProperty);
        }

        public static void SetAnimatedScroll(WrapPanel wrap, bool value)
        {


            wrap.SetValue(AnimatedScrollProperty, value);
        }

    }
    
    public static class ItemsPresenterExtensions
    {
        public static readonly AttachedProperty<bool> AnimatedScrollProperty =
            AvaloniaProperty.RegisterAttached<ItemsPresenter, bool>("AnimatedScroll", typeof(ItemsPresenter), defaultValue: false);

        static ItemsPresenterExtensions()
        {
            AnimatedScrollProperty.Changed.AddClassHandler<ItemsPresenter>(HandleAnimatedScrollChanged);
        }
        
        private static void HandleAnimatedScrollChanged(ItemsPresenter interactElem, AvaloniaPropertyChangedEventArgs args)
        {
            if(GetAnimatedScroll(interactElem))
                interactElem.AttachedToVisualTree += (sender, args) => Scrollable.MakeScrollable(ElementComposition.GetElementVisual(interactElem));
        }

        public static bool GetAnimatedScroll(ItemsPresenter wrap)
        {
            return wrap.GetValue(AnimatedScrollProperty);
        }

        public static void SetAnimatedScroll(ItemsPresenter wrap, bool value)
        {


            wrap.SetValue(AnimatedScrollProperty, value);
        }

    }

    
    public static class ItemsControlExtensions
    {
        public static readonly AttachedProperty<bool> AnimatedScrollProperty =
            AvaloniaProperty.RegisterAttached<ItemsControl, bool>("AnimatedScroll", typeof(ItemsControl), defaultValue: false);

        static ItemsControlExtensions()
        {
            AnimatedScrollProperty.Changed.AddClassHandler<ItemsControl>(HandleAnimatedScrollChanged);
        }
        
        private static void HandleAnimatedScrollChanged(ItemsControl interactElem, AvaloniaPropertyChangedEventArgs args)
        {
            if(GetAnimatedScroll(interactElem))
                interactElem.AttachedToVisualTree += (sender, args) => Scrollable.MakeScrollable(ElementComposition.GetElementVisual(interactElem));
        }

        public static bool GetAnimatedScroll(ItemsControl wrap)
        {
            return wrap.GetValue(AnimatedScrollProperty);
        }

        public static void SetAnimatedScroll(ItemsControl wrap, bool value)
        {


            wrap.SetValue(AnimatedScrollProperty, value);
        }

    }

  
  
}