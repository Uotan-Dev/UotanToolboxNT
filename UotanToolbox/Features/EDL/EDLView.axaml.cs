using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UotanToolbox.Common;
using UotanToolbox.Features.EDL;

namespace UotanToolbox.Features.EDL;

public partial class EDLView : UserControl
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public EDLView()
    {
        InitializeComponent();
        _ = SetEnabled();
    }

    public async Task SetEnabled()
    {
        while (true)
        {
            if (await GetDevicesInfo.SetDevicesInfoLittle())
            {
                MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
                if (sukiViewModel.Status != "9008")
                {
                    ReadPartTableBut.IsEnabled = false;
                    WritePartBut.IsEnabled = false;
                    ReadPartBut.IsEnabled = false;
                    ErasePartBut.IsEnabled = false;
                    MoreCard.IsEnabled = false;
                }
                else
                {
                    ReadPartTableBut.IsEnabled = true;
                    WritePartBut.IsEnabled = true;
                    ReadPartBut.IsEnabled = true;
                    ErasePartBut.IsEnabled = true;
                    MoreCard.IsEnabled = true;
                }
            }
            else
            {
                ReadPartTableBut.IsEnabled = false;
                WritePartBut.IsEnabled = false;
                ReadPartBut.IsEnabled = false;
                ErasePartBut.IsEnabled = false;
                MoreCard.IsEnabled = false;
            }
        }
    }

    private async void OpenImageFile(object sender, RoutedEventArgs args)
    {
        Button button = (Button)sender;
        EDLPartModel eDLPartModel = ( EDLPartModel )button.DataContext;
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            eDLPartModel.FileName = Path.GetFileName(StringHelper.FilePath(files[0].Path.ToString()));
            
            XDocument xdoc = XDocument.Load(Global.xml_path);
            var programElements = xdoc.Descendants("program");

            foreach (var element in programElements)
            {
                var indexAttribute = element.Attribute("Uotan-Index");
                if (indexAttribute != null && indexAttribute.Value == eDLPartModel.Index)
                {
                    var filenameAttribute = element.Attribute("filename");
                    if (filenameAttribute != null)
                    {
                        filenameAttribute.Value = StringHelper.FilePath(files[0].Path.ToString());
                    }
                }
            }

            xdoc.Save(Global.xml_path);
        }
    }
}