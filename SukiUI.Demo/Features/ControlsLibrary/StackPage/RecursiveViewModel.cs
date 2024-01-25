using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;

namespace SukiUI.Demo.Features.ControlsLibrary.StackPage
{
    public partial class RecursiveViewModel : ObservableObject, ISukiStackPageTitleProvider
    {
        public string Title { get; }

        private readonly int _index;
        private readonly Action<RecursiveViewModel> _onRecurseClicked;

        public RecursiveViewModel(int index, Action<RecursiveViewModel> onRecurseClicked)
        {
            _index = index;
            _onRecurseClicked = onRecurseClicked;
            Title = $"Recursive {index}";
        }

        [RelayCommand]
        public void Recurse() => 
            _onRecurseClicked.Invoke(new RecursiveViewModel(_index + 1, _onRecurseClicked));
    }
}