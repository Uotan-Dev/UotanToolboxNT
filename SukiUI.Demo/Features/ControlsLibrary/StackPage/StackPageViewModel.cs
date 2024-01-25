using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace SukiUI.Demo.Features.ControlsLibrary.StackPage
{
    public partial class StackPageViewModel : DemoPageBase
    {
        [ObservableProperty] private RecursiveViewModel _activeVm;
        [ObservableProperty] private int _limit = 5;

        public StackPageViewModel() : base("StackPage", MaterialIconKind.Layers)
        {
            _activeVm = new RecursiveViewModel(1, OnRecurseClicked);
        }

        private void OnRecurseClicked(RecursiveViewModel newRecursiveVm)
        {
            ActiveVm = newRecursiveVm;
        }
    }
}