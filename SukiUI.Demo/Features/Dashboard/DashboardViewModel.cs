using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Controls;
using SukiUI.Demo.Common;
using SukiUI.Demo.Models.Dashboard;
using System.Linq;
using System.Threading.Tasks;

namespace SukiUI.Demo.Features.Dashboard;

public partial class DashboardViewModel : DemoPageBase
{
    public AvaloniaList<string> SimpleContent { get; } = new();
    [ObservableProperty] private string _name;
    [ObservableProperty] private int _stepperIndex;
    [ObservableProperty] private string _selectedSimpleContent;

    public IAvaloniaReadOnlyList<InvoiceViewModel> Invoices { get; } = new AvaloniaList<InvoiceViewModel>()
    {
        new InvoiceViewModel(15364, "Jean", 156, true),
        new InvoiceViewModel(45689, "Fantine", 82, false),
        new InvoiceViewModel(15364, "Jean", 156, true),
        new InvoiceViewModel(45689, "Fantine", 82, false),
        new InvoiceViewModel(15364, "Jean", 156, true),
        new InvoiceViewModel(45689, "Fantine", 82, false),
    };

    public IAvaloniaReadOnlyList<string> Steps { get; } = new AvaloniaList<string>()
    {
        "Dispatched", "En-Route", "Delivered"
    };

    public DashboardViewModel() : base("Dashboard", MaterialIconKind.CircleOutline, -100)
    {
        StepperIndex = 1;
        SimpleContent.AddRange(["oem unlock", "oem unlock-go", "flashing unlock", "flashing unlock_critical"]);
    }

    [RelayCommand]
    public void ShowDialog()
    {
        SukiHost.ShowDialog(new DialogViewModel(), allowBackgroundClose: true);
    }

    [RelayCommand]
    public void IncrementIndex() =>
        StepperIndex += StepperIndex >= Steps.Count - 1 ? 0 : 1;

    [RelayCommand]
    public void DecrementIndex() =>
        StepperIndex -= StepperIndex <= 0 ? 0 : 1;
}