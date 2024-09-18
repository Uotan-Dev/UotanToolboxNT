using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Dialogs;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Customizedflash;

public partial class CustomizedflashView : UserControl
{
    static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);

    public CustomizedflashView() => InitializeComponent();

    ISukiDialogManager dialogManager;
    public async Task Fastboot(string fbshell)//Fastboot实时输出
    {
        await Task.Run(() =>
        {
            var cmd = Path.Combine(Global.bin_path, "platform-tools", "fastboot");

            var fastboot = new ProcessStartInfo(cmd, fbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var fb = new Process();
            fb.StartInfo = fastboot;
            _ = fb.Start();
            fb.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            fb.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            fb.BeginOutputReadLine();
            fb.BeginErrorReadLine();
            fb.WaitForExit();
            fb.Close();
        });
    }

    async void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var sb = new StringBuilder(CustomizedflashLog.Text);
                CustomizedflashLog.Text = sb.AppendLine(outLine.Data).ToString();
                CustomizedflashLog.ScrollToLine(StringHelper.TextBoxLine(CustomizedflashLog.Text));
            });
        }
    }

    async void OpenSystemFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            SystemFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async void FlashSystemFile(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            if (SystemFile.Text != null)
            {
                var sukiViewModel = GlobalData.MainViewModelInstance;

                if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
                {
                    Global.checkdevice = false;
                    OpenSystemFileBut.IsEnabled = false;
                    FlashSystemFileBut.IsEnabled = false;
                    CustomizedflashLog.Text = GetTranslation("Customizedflash_Flashing") + "\n";
                    var shell = string.Format($"-s {Global.thisdevice} flash system \"{SystemFile.Text}\"");
                    await Fastboot(shell);
                    OpenSystemFileBut.IsEnabled = true;
                    FlashSystemFileBut.IsEnabled = true;
                    Global.checkdevice = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Customizedflash_SelectFile"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void OpenProductFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            ProductFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async void FlashProductFile(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            if (ProductFile.Text != null)
            {
                var sukiViewModel = GlobalData.MainViewModelInstance;

                if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
                {
                    Global.checkdevice = false;
                    OpenProductFileBut.IsEnabled = false;
                    FlashProductFileBut.IsEnabled = false;
                    CustomizedflashLog.Text = GetTranslation("Customizedflash_Flashing") + "\n";
                    var shell = string.Format($"-s {Global.thisdevice} flash product \"{ProductFile.Text}\"");
                    await Fastboot(shell);
                    OpenProductFileBut.IsEnabled = true;
                    FlashProductFileBut.IsEnabled = true;
                    Global.checkdevice = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Customizedflash_SelectFile"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void OpenVenderFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            VenderFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async void FlashVenderFile(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            if (VenderFile.Text != null)
            {
                var sukiViewModel = GlobalData.MainViewModelInstance;

                if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
                {
                    Global.checkdevice = false;
                    OpenVenderFileBut.IsEnabled = false;
                    FlashVenderFileBut.IsEnabled = false;
                    CustomizedflashLog.Text = GetTranslation("Customizedflash_Flashing") + "\n";
                    var shell = string.Format($"-s {Global.thisdevice} flash vendor \"{VenderFile.Text}\"");
                    await Fastboot(shell);
                    OpenVenderFileBut.IsEnabled = true;
                    FlashVenderFileBut.IsEnabled = true;
                    Global.checkdevice = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Customizedflash_SelectFile"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void OpenBootFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            BootFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async void FlashBootFile(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            if (BootFile.Text != null)
            {
                var sukiViewModel = GlobalData.MainViewModelInstance;

                if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
                {
                    Global.checkdevice = false;
                    OpenBootFileBut.IsEnabled = false;
                    FlashBootFileBut.IsEnabled = false;
                    CustomizedflashLog.Text = GetTranslation("Customizedflash_Flashing") + "\n";
                    var shell = string.Format($"-s {Global.thisdevice} flash boot \"{BootFile.Text}\"");
                    await Fastboot(shell);
                    OpenBootFileBut.IsEnabled = true;
                    FlashBootFileBut.IsEnabled = true;
                    Global.checkdevice = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Customizedflash_SelectFile"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void OpenSystemextFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            SystemextFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async void FlashSystemextFile(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            if (SystemextFile.Text != null)
            {
                var sukiViewModel = GlobalData.MainViewModelInstance;

                if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
                {
                    Global.checkdevice = false;
                    OpenSystemextFileBut.IsEnabled = false;
                    FlashSystemextFileBut.IsEnabled = false;
                    CustomizedflashLog.Text = GetTranslation("Customizedflash_Flashing") + "\n";
                    var shell = string.Format($"-s {Global.thisdevice} flash system_ext \"{SystemextFile.Text}\"");
                    await Fastboot(shell);
                    OpenSystemextFileBut.IsEnabled = true;
                    FlashSystemextFileBut.IsEnabled = true;
                    Global.checkdevice = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Customizedflash_SelectFile"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void OpenOdmFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            OdmFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async void FlashOdmFile(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            if (OdmFile.Text != null)
            {
                var sukiViewModel = GlobalData.MainViewModelInstance;

                if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
                {
                    Global.checkdevice = false;
                    OpenOdmFileBut.IsEnabled = false;
                    FlashOdmFileBut.IsEnabled = false;
                    CustomizedflashLog.Text = GetTranslation("Customizedflash_Flashing") + "\n";
                    var shell = string.Format($"-s {Global.thisdevice} flash odm \"{OdmFile.Text}\"");
                    await Fastboot(shell);
                    OpenOdmFileBut.IsEnabled = true;
                    FlashOdmFileBut.IsEnabled = true;
                    Global.checkdevice = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Customizedflash_SelectFile"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void OpenVenderbootFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            VenderbootFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async void FlashVenderbootFile(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            if (VenderbootFile.Text != null)
            {
                var sukiViewModel = GlobalData.MainViewModelInstance;

                if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
                {
                    Global.checkdevice = false;
                    OpenVenderbootFileBut.IsEnabled = false;
                    FlashVenderbootFileBut.IsEnabled = false;
                    CustomizedflashLog.Text = GetTranslation("Customizedflash_Flashing") + "\n";
                    var shell = string.Format($"-s {Global.thisdevice} flash vendor_boot \"{VenderbootFile.Text}\"");
                    await Fastboot(shell);
                    OpenVenderbootFileBut.IsEnabled = true;
                    FlashVenderbootFileBut.IsEnabled = true;
                    Global.checkdevice = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Customizedflash_SelectFile"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void OpenInitbootFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            InitbootFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async void FlashInitbootFile(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            if (InitbootFile.Text != null)
            {
                var sukiViewModel = GlobalData.MainViewModelInstance;

                if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
                {
                    Global.checkdevice = false;
                    OpenInitbootFileBut.IsEnabled = false;
                    FlashInitbootFileBut.IsEnabled = false;
                    CustomizedflashLog.Text = GetTranslation("Customizedflash_Flashing") + "\n";
                    var shell = string.Format($"-s {Global.thisdevice} flash init_boot \"{InitbootFile.Text}\"");
                    await Fastboot(shell);
                    OpenInitbootFileBut.IsEnabled = true;
                    FlashInitbootFileBut.IsEnabled = true;
                    Global.checkdevice = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Customizedflash_SelectFile"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void OpenImageFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            ImageFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    async void FlashImageFile(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            if (ImageFile.Text != null)
            {
                var sukiViewModel = GlobalData.MainViewModelInstance;

                if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
                {
                    Global.checkdevice = false;
                    OpenImageFileBut.IsEnabled = false;
                    FlashImageFileBut.IsEnabled = false;
                    CustomizedflashLog.Text = GetTranslation("Customizedflash_Flashing") + "\n";
                    var shell = string.Format($"-s {Global.thisdevice} flash {Part.Text} \"{ImageFile.Text}\"");
                    await Fastboot(shell);
                    OpenImageFileBut.IsEnabled = true;
                    FlashImageFileBut.IsEnabled = true;
                    Global.checkdevice = true;
                }
                else
                {
                    _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
                }
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Customizedflash_SelectFile"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void DisableVbmeta(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                CustomizedflashLog.Text = "";
                var shell = string.Format($"-s {Global.thisdevice} --disable-verity --disable-verification flash vbmeta {Global.runpath}/Image/vbmeta.img");
                await Fastboot(shell);
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }

    async void SetOther(object sender, RoutedEventArgs args)
    {
        if (await GetDevicesInfo.SetDevicesInfoLittle())
        {
            var sukiViewModel = GlobalData.MainViewModelInstance;

            if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
            {
                CustomizedflashLog.Text = "";
                var shell = string.Format($"-s {Global.thisdevice} set_active other");
                await Fastboot(shell);
            }
            else
            {
                _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_EnterFastboot"))
.Dismiss().ByClickingBackground()
.TryShow();
            }
        }
        else
        {
            _ = dialogManager.CreateDialog()
.WithTitle("Error")
.OfType(NotificationType.Error)
.WithContent(GetTranslation("Common_NotConnected"))
.Dismiss().ByClickingBackground()
.TryShow();
        }
    }
}