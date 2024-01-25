using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using SukiUI.Content;
using System;
using System.Collections.Generic;

namespace SukiUI.Controls
{
    public class SukiStackPage : TemplatedControl
    {
        public static readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<SukiStackPage, object>(nameof(Content));

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly StyledProperty<int> LimitProperty =
            AvaloniaProperty.Register<SukiStackPage, int>(nameof(Limit), defaultValue: 5);

        public int Limit
        {
            get => GetValue(LimitProperty);
            set => SetValue(LimitProperty, value);
        }

        private static readonly DynamicResourceExtension ColorResource = new("SukiLowText");

        private StackPageModel?[]? _stackPages;
        private int _index = -1;

        private StackPanel? _stackHeaders;

        private readonly Stack<IDisposable> _disposables = new();

        public SukiStackPage()
        {
            _stackPages = new StackPageModel?[Limit];
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            UpdateHeaders();
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            while (_disposables.Count > 0)
                _disposables.Pop().Dispose();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            if (e.NameScope.Get<StackPanel>("StackHeader") is { } stackHeaders)
                _stackHeaders = stackHeaders;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ContentProperty)
                UpdateContentChanged(change.NewValue);
            else if (change.Property == LimitProperty && change.NewValue is int newLimit)
            {
                if (newLimit < 2)
                {
                    Limit = 2;
                    return;
                }

                UpdateStackArray(newLimit);
            }
        }

        private void UpdateStackArray(int newLimit)
        {
            var newArr = new StackPageModel?[newLimit];
            Array.Copy(_stackPages!, 0, newArr, 0, Math.Min(newArr.Length, _stackPages!.Length));
            _stackPages = newArr;
            var indexOfLastNotNull = Array.FindIndex(_stackPages, x => x is null) - 1;
            _index = indexOfLastNotNull > -1 ? indexOfLastNotNull : _stackPages.Length - 1;
            Content = _stackPages[_index]!.Content;
        }

        private void UpdateContentChanged(object? newVal)
        {
            if (newVal is null) return;
            if (_stackPages is null) return;
            var indexOfExists = Array.FindIndex(_stackPages, x => x is not null && x.Content == newVal);
            if (indexOfExists >= 0)
            {
                UnwindToIndex(indexOfExists);
                UpdateHeaders();
                return;
            }

            StackPageModel model;
            if (newVal is ISukiStackPageTitleProvider stackPageVm)
            {
                model = new StackPageModel(stackPageVm.Title, stackPageVm);
            }
            else if (newVal is Control c)
            {
                if (c.Name is not null)
                    model = new StackPageModel(c.Name, newVal);
                else
                {
                    model = new StackPageModel(c.Name, newVal);
                    c.AttachedToVisualTree += (sender, args) => model.Title = c.Name;
                }
            }
            else
                model = new StackPageModel(newVal.GetType().Name, newVal);
            if (_index >= _stackPages.Length - 1)
                ShiftLeft();
            _index++;
            _stackPages[_index] = model;
            UpdateHeaders();
        }

        private void UnwindToIndex(int index)
        {
            Array.Copy(_stackPages, 0, _stackPages, 0, index + 1);
            _index = index;
        }

        private void ShiftLeft()
        {
            Array.Copy(_stackPages, 1, _stackPages, 0, _stackPages.Length - 1);
            _index = _stackPages.Length - 2;
        }

        private void UpdateHeaders()
        {
            if (_stackHeaders is null) return;
            _stackHeaders.Children.Clear();
            if (_index == -1 || _stackPages?[0] == null) return;
            for (var i = 0; i < _index; i++)
            {
                AddLowHeader(_stackPages[i]!);
                AddChevron();
            }

            AddStrongHeader(_stackPages[_index]!.Title);
        }

        private void AddChevron()
        {
            var pathIcon = new PathIcon
            {
                Data = Icons.ChevronRight,
                Height = 15,
                Width = 15,
                Margin = new Thickness(15, -3, 15, 0),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            _disposables.Push(pathIcon.Bind(ForegroundProperty, ColorResource));
            _stackHeaders!.Children.Add(pathIcon);
        }

        private void AddLowHeader(StackPageModel model)
        {
            var button = new TextBlock()
            {
                Classes = { "h2" },
                Text = model.Title,
            };

            _disposables.Push(button.Bind(ForegroundProperty, ColorResource));

            button.PointerReleased += (_, _) =>
                Content = model.Content;

            button.PointerEntered += (_, _) =>
                button.RenderTransform = new ScaleTransform() { ScaleX = 1.02, ScaleY = 1.02 };

            button.PointerExited += (_, _) =>
                button.RenderTransform = new ScaleTransform() { ScaleX = 1, ScaleY = 1 };

            _stackHeaders!.Children.Add(button);
        }

        private void AddStrongHeader(string s)
        {
            _stackHeaders!.Children.Add(new TextBlock()
            {
                Classes = { "h2" },
                Text = s
            });
        }
    }

    internal record StackPageModel(string Title, object Content)
    {
        public string Title { get; set; } = Title;
        public object Content { get; } = Content;
    }

    public interface ISukiStackPageTitleProvider
    {
        public string Title { get; }
    }
}