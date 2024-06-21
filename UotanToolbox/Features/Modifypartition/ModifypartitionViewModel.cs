using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace UotanToolbox.Features.Modifypartition;

public partial class ModifypartitionViewModel : MainPageBase
{
    public AvaloniaList<DataGridContentViewModel> DataGridContent { get; } = [];

    private static readonly ResourceManager resMgr = new ResourceManager("UotanToolbox.Assets.Resources", typeof(App).Assembly);
    private static string GetTranslation(string key) => resMgr.GetString(key, CultureInfo.CurrentCulture) ?? "?????";

    public ModifypartitionViewModel() : base(GetTranslation("Sidebar_ModifyPartition"), MaterialIconKind.WrenchCogOutline, -400)
    {
        DataGridContent.AddRange(Enumerable.Range(1, 50).Select(x => new DataGridContentViewModel(x)));
    }

    public partial class DataGridContentViewModel(int value) : ObservableObject
    {
        [ObservableProperty]
        private string _idColumn = $"Content {value}",
            _startpointColumn = $"Content {value}",
            _endpointColumn = $"Content {value}",
            _sizeColumn = $"Content {value}",
            _formatColumn = $"Content {value}",
            _nameColumn = $"Content {value}",
            _signColumn = $"Content {value}";
    }

}