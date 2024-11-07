using SukiUI.Controls;
using System.Collections.ObjectModel;

namespace UotanToolbox.Features.Rom;

public partial class RomProject : SukiWindow
{
    public RomProject()
    {
        InitializeComponent();
        Project[] projects = new Project[13];
        ImageList.ItemsSource = new ObservableCollection<Project>(projects);
    }
}