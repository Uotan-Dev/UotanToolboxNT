using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using SukiUI.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Interactivity;

namespace SukiUI.Controls
{
    public class Stepper : TemplatedControl
    {
        public static readonly StyledProperty<bool> AlternativeStyleProperty =
            AvaloniaProperty.Register<Stepper, bool>(nameof(AlternativeStyle), defaultValue: false);

        public bool AlternativeStyle
        {
            get { return GetValue(AlternativeStyleProperty); }
            set { SetValue(AlternativeStyleProperty, value); }
        }

        
        public static readonly StyledProperty<int> IndexProperty =
            AvaloniaProperty.Register<Stepper, int>(nameof(Index));

        public int Index
        {
            get => GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
        }

        public static readonly StyledProperty<IEnumerable?> StepsProperty =
            AvaloniaProperty.Register<Stepper, IEnumerable?>(nameof(Steps));

        public IEnumerable? Steps
        {
            get => GetValue(StepsProperty);
            set => SetValue(StepsProperty, value);
        }

        private Grid? _grid;
        private IDisposable? _subscriptionDisposables;

        private static readonly IBrush DisabledColor = new SolidColorBrush(Color.FromArgb(100, 150, 150, 150));

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if (e.NameScope.Get<Grid>("PART_GridStepper") is not { } grid) return;
            _grid = grid;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            var indexObs = this.GetObservable(IndexProperty)
                .Do(_ => StepsChangedHandler(Steps))
                .Select(_ => Unit.Default);
            _subscriptionDisposables = this.GetObservable(StepsProperty)
                .Do(_ => StepsChangedHandler(Steps))
                .Select(_ => Unit.Default)
                .Merge(indexObs)
                .ObserveOn(new AvaloniaSynchronizationContext())
                .Subscribe();
        }

        private void StepsChangedHandler(IEnumerable? newSteps)
        {
            if (newSteps is null) return;
            if (newSteps is not IEnumerable<object> stepsEnumerable) return;
            var steps = stepsEnumerable.ToArray();
            
            if(AlternativeStyle)
                UpdateAlternate(steps);    
            else 
                Update(steps);
            
            if (newSteps is INotifyCollectionChanged notify)
                notify.CollectionChanged += (_, _) => Update(steps);
        }

        #region  StepperBaseStyle
        private void Update(object[] steps)
        {
            if (_grid is null) return;
            _grid.Children.Clear();

            SetColumnDefinitions(_grid, steps);

            for (var i = 0; i < steps.Length; i++)
                AddStep(steps[i], i, _grid, steps.Length);
        }

        private void SetColumnDefinitions(Grid grid, object[] steps)
        {
            var columns = new ColumnDefinitions();
            for (int i = 0; i < steps.Length; i++)
            {
                columns.Add(new ColumnDefinition());
            }

            grid.ColumnDefinitions = columns;
        }

        private void AddStep(object step, int index, Grid grid, int stepCount)
        {
            var griditem = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitions()
                    { new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto) }
            };

            var icon = new PathIcon()
                { Height = 10, Width = 10, Data = Icons.ChevronRight, Margin = new Thickness(0, 0, 20, 0) };
            if (index == stepCount - 1)
                icon.IsVisible = false;

            Grid.SetColumn(icon, 2);
            griditem.Children.Add(icon);

            var circle = new Border()
            {
                Margin = new Thickness(0, 0, 0, 2),
                Height = 24,
                Width = 24,
                CornerRadius = new CornerRadius(25),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (index <= Index)
            {
                circle[!BackgroundProperty] = new DynamicResourceExtension("SukiPrimaryColor");

                circle.BorderThickness = new Thickness(0);
                circle.Child = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = (index + 1).ToString(),
                    FontSize = 13,
                    Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap
                };
            }
            else
            {
                circle.Background = DisabledColor;

                circle.BorderThickness = new Thickness(0);
                circle.Child = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = (index + 1).ToString(),
                    FontSize = 13,
                    Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap
                };
            }

            Grid.SetColumn(circle, 0);
            griditem.Children.Add(circle);
            Control content = step switch
            {
                string s => new TextBlock()
                {
                    FontWeight = index <= Index ? FontWeight.DemiBold : FontWeight.Normal,
                    Margin = new Thickness(10, 0, 0, 0),
                    Text = s,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left, TextWrapping = TextWrapping.Wrap
                },
                _ => new ContentControl() { Content = step }
            };

            Grid.SetColumn(content, 1);
            griditem.Children.Add(content);

            Grid.SetColumn(griditem, index);

            grid.Children.Add(griditem);
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            _subscriptionDisposables?.Dispose();
        }
        
        #endregion

        #region  StepperAlternateStyle

        
        public void UpdateAlternate(object[] steps)
        {
            try
            {
                if (_grid is null) return;
                _grid.Children.Clear();

                SetColumnDefinitionsAlternate(_grid);

                for (var i = 0; i < steps.Length; i++)
                {
                    AddStepAlternate(steps[i], i, _grid, steps);
                }
            }catch(Exception exc) { }
        }

        private void SetColumnDefinitionsAlternate(Grid grid)
        {
            var columns = new ColumnDefinitions();
            foreach(var s in Steps)
                columns.Add(new ColumnDefinition());
            grid.ColumnDefinitions = columns;
        }

        private void AddStepAlternate(object step, int index,Grid grid,object[] steps)
        {

         
            Brush PrimaryColor = new SolidColorBrush( (Color)Application.Current.FindResource("SukiPrimaryColor"));
            Brush DisabledColor =  new SolidColorBrush( (Color)Application.Current.FindResource("SukiControlBorderBrush"));
            
            var griditem = new Grid(){ ColumnDefinitions = new ColumnDefinitions(){new ColumnDefinition(), new ColumnDefinition()}};

            var line = new Border() { CornerRadius = new CornerRadius(3),  Margin = new Thickness(-5,0,23,0), Background = DisabledColor, Height = 2, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center };
            var line1 = new Border() { CornerRadius = new CornerRadius(3),  Margin = new Thickness(23,0,-5,0), Background = DisabledColor, Height = 2, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Center };

            if (index == 0)
                line.IsVisible = false;
            if (index == steps.Length -1)
                line1.IsVisible = false;

            if (index == Index)
                line.Background = PrimaryColor;

            if (index < Index)
            {
                line1.Background = PrimaryColor;
                line.Background = PrimaryColor;
            }
            
            Grid.SetColumn(line,0);
            Grid.SetColumn(line1,1);
            
            griditem.Children.Add(line);
            griditem.Children.Add(line1);

            var gridBorder = new Grid();
            
            var circle = new Border()
                { Margin = new Thickness(0,0,0,2), Height = 30, Width = 30, CornerRadius = new CornerRadius(25), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            if (index == Index)
            {
                circle.Background = PrimaryColor;
                
                circle.BorderThickness = new Thickness(0);
                circle.Child = new TextBlock() {VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Text = (index + 1).ToString(), Foreground = Brushes.White};
            }
            else if (index < Index)
            {
                circle.Background = Brushes.Transparent;
                circle.BorderThickness = new Thickness(1.5);
                circle.BorderBrush = PrimaryColor;
                circle.Child = new TextBlock() {VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Text = (index + 1).ToString(), Foreground = PrimaryColor};
            }
            else
            {
                circle.Background = Brushes.Transparent;
                circle.BorderThickness = new Thickness(1.5);
                circle.BorderBrush = DisabledColor;
                circle.Child = new TextBlock() { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Text = (index + 1).ToString(), Foreground = DisabledColor};
            }
            

            
            
            gridBorder.Children.Add(circle);
            
            gridBorder.Children.Add(new TextBlock()
            {
                FontWeight = index == Index ? FontWeight.Medium: FontWeight.Normal,
                Text = step.ToString(), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0,55,0,0)
            });
            
            Grid.SetColumn(griditem,index);
            Grid.SetColumn(gridBorder,index);
            grid.Children.Add(griditem);
            grid.Children.Add(gridBorder);
        }

        #endregion

    }
}