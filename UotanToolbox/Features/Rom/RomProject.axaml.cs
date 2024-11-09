using System.Collections.ObjectModel;
using SukiUI.Controls;

namespace UotanToolbox.Features.Rom;

public partial class RomProject : SukiWindow
{
    public RomProject()
    {
        InitializeComponent();
        Project[] projects = new Project[23];
        ImageList.ItemsSource = new ObservableCollection<Project>(projects);
    }
}