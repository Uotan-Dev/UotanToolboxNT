using System;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace SukiUI.Helpers
{
    public class CompositionAnimationHelper
    {
        public static void MakeScrollable(CompositionVisual compositionVisual, double millis = 250)
        {
            if (compositionVisual == null)
                return;
        
            Compositor compositor = compositionVisual.Compositor;

            var animationGroup = compositor.CreateAnimationGroup();
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = "Offset";

            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(millis);

            ImplicitAnimationCollection implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
            animationGroup.Add(offsetAnimation);
            implicitAnimationCollection["Offset"] = animationGroup;
            compositionVisual.ImplicitAnimations = implicitAnimationCollection;
        }
        
            public static void MakeOpacityAnimated(CompositionVisual compositionVisual, double millis = 700)
    {
        if (compositionVisual == null)
            return;

        Compositor compositor = compositionVisual.Compositor;

        var animationGroup = compositor.CreateAnimationGroup();
      
        
        ScalarKeyFrameAnimation opacityAnimation = compositor.CreateScalarKeyFrameAnimation();
        opacityAnimation.Target = "Opacity";
        opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        opacityAnimation.Duration = TimeSpan.FromMilliseconds(millis);
        
    
       
        Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(millis);
        

        animationGroup.Add(offsetAnimation);
        animationGroup.Add(opacityAnimation);
      
        
        ImplicitAnimationCollection implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
        implicitAnimationCollection["Opacity"] = animationGroup;
        implicitAnimationCollection["Offset"] = animationGroup;

        
        compositionVisual.ImplicitAnimations = implicitAnimationCollection;

    }

    public static void MakeSizeAnimated(CompositionVisual compositionVisual, double millis =450)
    {
        if (compositionVisual == null)
            return;

        Compositor compositor = compositionVisual.Compositor;

        var animationGroup = compositor.CreateAnimationGroup();
        
        Vector2KeyFrameAnimation sizeAnimation = compositor.CreateVector2KeyFrameAnimation();
        sizeAnimation.Target = "Size";
        sizeAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        sizeAnimation.Duration = TimeSpan.FromMilliseconds(millis);
        
      
        
        Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(millis);
        
        
        
        animationGroup.Add(sizeAnimation);

        animationGroup.Add(offsetAnimation);
        
        ImplicitAnimationCollection implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
        implicitAnimationCollection["Size"] = animationGroup;
        implicitAnimationCollection["Offset"] = animationGroup;

        
        compositionVisual.ImplicitAnimations = implicitAnimationCollection;

    }
    
    
    public static void MakeSizeOpacityAnimated(CompositionVisual compositionVisual, double millis =450)
    {
        if (compositionVisual == null)
            return;

        Compositor compositor = compositionVisual.Compositor;

        var animationGroup = compositor.CreateAnimationGroup();
        
        Vector2KeyFrameAnimation sizeAnimation = compositor.CreateVector2KeyFrameAnimation();
        sizeAnimation.Target = "Size";
        sizeAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        sizeAnimation.Duration = TimeSpan.FromMilliseconds(millis);
        
      
        
        Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(millis);
        
        
        ScalarKeyFrameAnimation opacityAnimation = compositor.CreateScalarKeyFrameAnimation();
        opacityAnimation.Target = "Opacity";
        opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        opacityAnimation.Duration = TimeSpan.FromMilliseconds(millis);

        
        
        animationGroup.Add(sizeAnimation);
        animationGroup.Add(opacityAnimation);

        animationGroup.Add(offsetAnimation);
        
        ImplicitAnimationCollection implicitAnimationCollection = compositor.CreateImplicitAnimationCollection();
        implicitAnimationCollection["Size"] = animationGroup;
        implicitAnimationCollection["Opacity"] = animationGroup;
        implicitAnimationCollection["Offset"] = animationGroup;

        
        compositionVisual.ImplicitAnimations = implicitAnimationCollection;

    }

    }
}