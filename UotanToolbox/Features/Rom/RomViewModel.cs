using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Rom;

public partial class RomViewModel : MainPageBase
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    [ObservableProperty]
    private ObservableCollection<Project> projectList = null;
    [ObservableProperty]
    private bool typeEXT4 = false, formatSPARSE = false, eXT4RO = false, sizeOri = false, eXT4Compress = false, eROFSLZ4HC = false, sUPERSPARSE = false,
        typeEROFS = true, formatRAW = true, eXT4RW = true, sizeAuto = true, eXT4NoCompress = true, eROFSLZ4 = true, sUPERRAW = true;
    [ObservableProperty]
    private int eROFSComGrade = 8, bRComGrade = 3;
    [ObservableProperty]
    private string projectName = "", sUPERSize = "9126805504", sUPERName = "qti_dynamic_partitions", uTCTime = "1230768000", dATSeg = "15";

    public RomViewModel() : base("ROM工坊", MaterialIconKind.ChartPieOutline, -250)
    {
        Project[] projects = new Project[10];
        projects[0] = new Project { ProjectName = "muyu-ota_full-OS2.0.4.0.VOYCNXM-user-15.0-4152616bf9" };
        projects[1] = new Project { ProjectName = "sheng-ota_full-OS2.0.1.10.VNXCNXM-user-15.0-25b489e98b" };
        ProjectList = new ObservableCollection<Project>(projects);
    }

    [RelayCommand]
    public async Task DeleteProject()
    {

    }

    [RelayCommand]
    public async Task NewProject()
    {

    }

    [RelayCommand]
    public async Task ComROM()
    {

    }

    [RelayCommand]
    public async Task ScriptPort()
    {

    }

    [RelayCommand]
    public async Task CardToWired()
    {

    }

    [RelayCommand]
    public async Task InstallPlug()
    {

    }

    [RelayCommand]
    public async Task OpenProject()
    {

    }
}

public partial class Project : ObservableObject
{
    [ObservableProperty]
    private string projectName;

    [ObservableProperty]
    private bool isSelected;
}