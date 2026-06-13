using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Common.Devices;

namespace UotanToolbox.Features.Filemgr;

/// <summary>
/// <para>剪贴板操作类型枚举，用于区分复制与剪切操作。</para>
/// Enum for clipboard operation types, distinguishing between copy and cut operations.
/// </summary>
internal enum ClipboardOperation
{
    /// <summary>
    /// <para>无操作。</para>
    /// No operation.
    /// </summary>
    None,
    /// <summary>
    /// <para>复制操作。</para>
    /// Copy operation.
    /// </summary>
    Copy,
    /// <summary>
    /// <para>剪切操作。</para>
    /// Cut operation.
    /// </summary>
    Cut
}

/// <summary>
/// <para>文件管理页面的视图模型，提供设备文件浏览与导航功能。</para>
/// View model for the file management page, providing device file browsing and navigation capabilities.
/// </summary>
public partial class FilemgrViewModel : MainPageBase
{
    [ObservableProperty]
    private bool _useROOT;
    /// <summary>
    /// <para>用于显示的文件条目列表。</para>
    /// The file entry list for display.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<FileEntry> _files = [];

    /// <summary>
    /// <para>设备上的当前目录路径，默认为根目录 "/"。</para>
    /// The current directory path on the device, defaults to root "/".
    /// </summary>
    [ObservableProperty]
    private string _currentPath = "/";

    /// <summary>
    /// <para>指示是否正在加载文件列表。</para>
    /// Indicates whether the file list is being loaded.
    /// </summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// <para>路径输入框中的文本，与 CurrentPath 同步。</para>
    /// The text in the path input box, synced with CurrentPath.
    /// </summary>
    [ObservableProperty]
    private string _pathInput = "/";

    /// <summary>
    /// <para>当前选中的文件条目，用于上下文菜单操作。</para>
    /// The currently selected file entry for context menu operations.
    /// </summary>
    [ObservableProperty]
    private FileEntry _selectedFile;

    /// <summary>
    /// <para>快捷访问目录列表。</para>
    /// The quick access directory list.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<QuickAccessItem> _quickAccessItems = [];

    /// <summary>
    /// <para>指示文件列表中是否有条目，用于控制空列表提示的可见性。</para>
    /// Indicates whether the file list has items, controlling the visibility of the empty list hint.
    /// </summary>
    [ObservableProperty]
    private bool _hasItems;

    /// <summary>
    /// <para>当前选中的文件条目数量，用于批量操作提示。</para>
    /// The count of currently selected file entries, used for batch operation hints.
    /// </summary>
    [ObservableProperty]
    private int _selectedCount;

    /// <summary>
    /// <para>指示是否可以执行粘贴操作（剪贴板中有内容时为 true）。</para>
    /// Indicates whether a paste operation can be performed (true when the clipboard has content).
    /// </summary>
    [ObservableProperty]
    private bool _canPaste;

    /// <summary>
    /// <para>指示当前是否有设备连接。</para>
    /// Indicates whether a device is currently connected.
    /// </summary>
    [ObservableProperty]
    private bool _isDeviceConnected;

    string RootMode { get; set; }

    /// <summary>
    /// <para>剪贴板中存储的文件条目列表，支持批量操作。</para>
    /// The list of file entries stored in the clipboard, supporting batch operations.
    /// </summary>
    private List<FileEntry> _clipboardEntries = [];

    /// <summary>
    /// <para>剪贴板操作类型（复制、剪切或无）。</para>
    /// The clipboard operation type (copy, cut, or none).
    /// </summary>
    private ClipboardOperation _clipboardOperation = ClipboardOperation.None;

    /// <summary>
    /// <para>指示是否已加载过目录（自动或手动），避免重复触发自动加载。</para>
    /// Indicates whether a directory has been loaded (auto or manual), preventing duplicate auto-load triggers.
    /// </summary>
    [ObservableProperty]
    private bool _hasLoaded;

    /// <summary>
    /// <para>获取是否可以向上导航到父目录。</para>
    /// Gets whether navigation to the parent directory is possible.
    /// </summary>
    public bool CanNavigateUp => !string.IsNullOrEmpty(CurrentPath) && CurrentPath != "/";

    /// <summary>
    /// <para>当前页面的全局变量，记录上一次检测到的设备形态 (Android 或 OpenHOS)。</para>
    /// </summary>
    private string _currentDeviceState = string.Empty;

    /// <summary>
    /// <para>根据 MainViewModel 的 Status 判断设备形态，发生变化时实时切换快捷访问目录路径。</para>
    /// </summary>
    private void UpdateQuickAccessPathsByDeviceState()
    {
        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        if (sukiViewModel == null) return;

        // 获取最新状态，判断是 Android 还是 鸿蒙 (OpenHOS)
        string newDeviceState = "Android"; // 默认为 Android
        if (sukiViewModel.Status == GetTranslation("Home_OpenHOS"))
        {
            newDeviceState = "OpenHOS";
        }
        else if (sukiViewModel.Status == GetTranslation("Home_Android") || sukiViewModel.Status.Contains("Android"))
        {
            newDeviceState = "Android";
        }

        // 仅当设备形态发生变化时，才触发路径修改操作
        if (_currentDeviceState != newDeviceState)
        {
            _currentDeviceState = newDeviceState;
            bool isHdc = _currentDeviceState == "OpenHOS";

            foreach (var item in QuickAccessItems)
            {
                if (isHdc && item.Path.StartsWith("/sdcard"))
                {
                    item.Path = item.Path.Replace("/sdcard", "/storage/media/100/local/files/Docs");
                    if (item.Name == GetTranslation("Filemgr_QA_Pictures"))
                    {
                        item.Path = item.Path.Replace("/Pictures", "/Images");
                    }
                }
                else if (!isHdc && item.Path.StartsWith("/storage/media/100/local/files/Docs"))
                {
                    item.Path = item.Path.Replace("/storage/media/100/local/files/Docs", "/sdcard");
                    if (item.Name == GetTranslation("Filemgr_QA_Pictures"))
                    {
                        item.Path = item.Path.Replace("/Images", "/Pictures");
                    }
                }
            }
        }
    }

    /// <summary>
    /// <para>检查设备连接，并同步更新快捷访问路径形态。</para>
    /// </summary>
    private async Task<bool> EnsureDeviceConnectedAndRefreshStateAsync()
    {
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            IsDeviceConnected = false;
            await ShowNotConnectedDialogAsync();
            return false;
        }

        IsDeviceConnected = true;
        // 成功获取设备信息后，触发路径状态检测和修改
        UpdateQuickAccessPathsByDeviceState();
        return true;
    }

    /// <summary>
    /// <para>当 CurrentPath 属性变化时，更新快捷访问项的高亮状态。</para>
    /// Updates the active state of quick access items when CurrentPath changes.
    /// </summary>
    partial void OnCurrentPathChanged(string value)
    {
        OnPropertyChanged(nameof(CanNavigateUp));
        foreach (var item in QuickAccessItems)
        {
            item.IsActive = string.Equals(item.Path, value, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// <para>当 Files 属性变化时，重新订阅集合变更通知以更新 HasItems。</para>
    /// Re-subscribes to collection change notifications when the Files property changes to update HasItems.
    /// </summary>
    partial void OnFilesChanged(ObservableCollection<FileEntry> value)
    {
        if (value is not null)
        {
            value.CollectionChanged += OnFilesCollectionChanged;
        }
        UpdateHasItems(value);
    }

    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    /// <summary>
    /// <para>初始化 <see cref="FilemgrViewModel"/> 的新实例，设置显示名称、图标及快捷访问项。</para>
    /// Initializes a new instance of <see cref="FilemgrViewModel"/>, setting display name, icon, and quick access items.
    /// </summary>
    public FilemgrViewModel() : base(GetTranslation("Sidebar_Filemgr"), MaterialIconKind.FolderOutline, -625)
    {
        // Built-in quick access items
        var builtInItems = new List<QuickAccessItem>
        {
            new() { Name = GetTranslation("Filemgr_QA_Root"), Path = "/", Icon = MaterialIconKind.HomeOutline },
            new() { Name = GetTranslation("Filemgr_QA_Internal"), Path = "/sdcard", Icon = MaterialIconKind.Harddisk },
            new() { Name = GetTranslation("Filemgr_QA_Download"), Path = "/sdcard/Download", Icon = MaterialIconKind.DownloadOutline },
            new() { Name = GetTranslation("Filemgr_QA_Documents"), Path = "/sdcard/Documents", Icon = MaterialIconKind.FileDocumentOutline },
            new() { Name = GetTranslation("Filemgr_QA_Pictures"), Path = "/sdcard/Pictures", Icon = MaterialIconKind.ImageOutline },
            new() { Name = "TMP", Path = "/data/local/tmp", Icon = MaterialIconKind.FolderClockOutline },
        };

        // Load custom quick access items from persistence
        var customData = QuickAccessPersistence.Load();
        var customItems = QuickAccessPersistence.ToQuickAccessItems(customData);

        var allItems = new ObservableCollection<QuickAccessItem>(builtInItems.Concat(customItems));
        QuickAccessItems = allItems;

        // Subscribe to Files collection changes
        Files.CollectionChanged += OnFilesCollectionChanged;
        UpdateHasItems(Files);

        // Subscribe to device manager events for auto-loading when device connects
        if (Global.DeviceManager != null)
        {
            Global.DeviceManager.ScanCompleted += OnScanCompleted;
            Global.DeviceManager.DeviceRemoved += OnDeviceRemoved;
        }

        // Subscribe to active page changes for auto-loading when switching to this page
        if (GlobalData.MainViewModelInstance != null)
        {
            GlobalData.MainViewModelInstance.PropertyChanged += OnMainViewModelPropertyChanged;
        }
    }

    /// <summary>
    /// <para>检查当前页面是否为活动页面。</para>
    /// Checks whether this page is the currently active page.
    /// </summary>
    private bool IsActivePage()
    {
        return GlobalData.MainViewModelInstance?.ActivePage == this;
    }

    /// <summary>
    /// <para>检查是否有支持 shell 命令的已连接设备（ADB 或 HDC）。</para>
    /// Checks whether there is a connected device that supports shell commands (ADB or HDC).
    /// </summary>
    private bool HasConnectedAdbDevice()
    {
        var device = Global.DeviceManager?.Devices.FirstOrDefault(d => d.Id == Global.thisdevice);
        return device != null && (device.Transport == TransportType.Adb || device.Transport == TransportType.Hdc);
    }

    /// <summary>
    /// <para>当设备扫描完成时，如果当前页面为活动页面且有可用设备则自动加载内部存储目录。</para>
    /// When a device scan completes, auto-loads the internal storage directory if this page is active and a device is available.
    /// </summary>
    private void OnScanCompleted(object? sender, EventArgs e)
    {
        if (IsActivePage() && !HasLoaded && HasConnectedAdbDevice())
        {
            _ = TryAutoLoadAsync();
        }

        IsDeviceConnected = Global.DeviceManager?.Devices.Any() == true;
    }

    /// <summary>
    /// <para>当设备移除时，如果移除的是当前设备则重置加载状态并清空文件列表。</para>
    /// When a device is removed, resets the loaded state and clears the file list if the removed device is the current one.
    /// </summary>
    private void OnDeviceRemoved(object? sender, DeviceEventArgs e)
    {
        // If the removed device is the currently selected one, reset state and clear files
        if (e.Device.Id == Global.thisdevice)
        {
            HasLoaded = false;
            IsDeviceConnected = false;
            Dispatcher.UIThread.Post(() =>
            {
                Files.Clear();
                CurrentPath = "/";
                PathInput = "/";
            });
        }
        else
        {
            IsDeviceConnected = Global.DeviceManager?.Devices.Any() == true;
        }
    }

    /// <summary>
    /// <para>当 MainViewModel 的属性变化时，检测页面切换并自动加载。</para>
    /// When MainViewModel properties change, detects page switching and auto-loads.
    /// </summary>
    private void OnMainViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.ActivePage) && IsActivePage())
        {
            IsDeviceConnected = Global.DeviceManager?.Devices.Any() == true;
            if (!HasLoaded)
            {
                var device = Global.DeviceManager?.Devices.FirstOrDefault(d => d.Id == Global.thisdevice);
                if (device != null && (device.Transport == TransportType.Adb || device.Transport == TransportType.Hdc))
                {
                    _ = TryAutoLoadAsync();
                }
            }
        }
    }

    /// <summary>
    /// <para>尝试自动加载内部存储目录。仅在尚未加载过时执行。</para>
    /// Attempts to auto-load the internal storage directory. Only executes when not yet loaded.
    /// </summary>
    private async Task TryAutoLoadAsync()
    {
        if (HasLoaded)
            return;

        HasLoaded = true;

        // 确保获取一次最新状态并更新路径
        await GetDevicesInfo.SetDevicesInfoLittle();
        UpdateQuickAccessPathsByDeviceState();

        bool isHdc = _currentDeviceState == "OpenHOS";
        string defaultPath = isHdc ? "/storage/media/100/local/files/Docs" : "/sdcard";

        await LoadDirectoryAsync(defaultPath);
    }

    /// <summary>
    /// <para>当 Files 集合内容变更时更新 HasItems 属性。</para>
    /// Updates the HasItems property when the Files collection content changes.
    /// </summary>
    private void OnFilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateHasItems(Files);
        UpdateSelectedCount();
    }

    /// <summary>
    /// <para>根据文件列表是否包含条目更新 HasItems 属性。</para>
    /// Updates the HasItems property based on whether the file list contains items.
    /// </summary>
    private void UpdateHasItems(ObservableCollection<FileEntry>? files)
    {
        HasItems = files is not null && files.Count > 0;
    }

    /// <summary>
    /// <para>更新选中条目计数。</para>
    /// Updates the selected entry count.
    /// </summary>
    private void UpdateSelectedCount()
    {
        SelectedCount = Files.Count(f => f.IsSelected);
    }

    /// <summary>
    /// <para>保存自定义快捷访问项到持久化存储。</para>
    /// Saves custom quick access items to persistent storage.
    /// </summary>
    private void SaveCustomQuickAccessItems()
    {
        var customData = QuickAccessPersistence.FromQuickAccessItems(QuickAccessItems);
        QuickAccessPersistence.Save(customData);
    }

    [RelayCommand]
    public async Task SetRootMode()
    {
        if (UseROOT)
        {
            Global.MainDialogManager.CreateDialog()
                            .WithTitle(GetTranslation("Common_Warn"))
                            .WithContent(GetTranslation("Common_NeedRoot"))
                            .OfType(NotificationType.Warning)
                            .WithActionButton(GetTranslation("Common_DebugMode"), async _ =>
                            {
                                RootMode = "debug";
                            }, true)
                            .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                            {
                                RootMode = "root";
                            }, true)
                            .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ =>
                            {
                                RootMode = "no";
                                UseROOT = false;
                            }, true)
                            .TryShow();
        }
        else
        {
            RootMode = "no";
        }
    }

    public async Task<string> RunADB(string cmd, bool isShell = true)
    {
        bool isHdc = false;
        if (Global.DeviceManager != null && !string.IsNullOrEmpty(Global.thisdevice))
        {
            var dev = Global.DeviceManager.Devices.FirstOrDefault(d => d.Id == Global.thisdevice);
            if (dev != null && dev.Transport == TransportType.Hdc)
            {
                isHdc = true;
            }
        }

        if (!isShell)
        {
            if (isHdc)
            {
                if (cmd.StartsWith("push "))
                {
                    cmd = "file send" + cmd.Substring(4);
                }
                else if (cmd.StartsWith("pull "))
                {
                    cmd = "file recv" + cmd.Substring(4);
                }
                return await FeaturesHelper.HdcCmd(Global.thisdevice, cmd);
            }
            return await FeaturesHelper.AdbCmd(Global.thisdevice, cmd);
        }

        if (isHdc)
        {
            return await FeaturesHelper.HdcCmd(Global.thisdevice, $"shell {cmd}");
        }

        if (RootMode == "debug")
        {
            await FeaturesHelper.AdbCmd(Global.thisdevice, "root");
            return await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell {cmd}");
        }
        else if (RootMode == "root")
        {
            return await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell su -c \"{cmd}\"");
        }
        else
        {
            return await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell {cmd}");
        }
    }

        /// <summary>
        /// <para>加载指定路径的目录内容。检查设备连接、执行 ADB 命令、解析输出并更新文件列表。</para>
        /// Loads the directory contents at the specified path. Checks device connection, executes ADB command, parses output, and updates the file list.
        /// </summary>
        /// <param name="path">
        /// <para>要加载的设备目录路径。</para>
        /// The device directory path to load.
        /// </param>
        [RelayCommand]
    public async Task LoadDirectoryAsync(string path)
    {
        // Set busy state immediately for responsive UI feedback
        IsBusy = true;

        if (!await GetDevicesInfo.SetDevicesInfoLittle())
        {
            IsDeviceConnected = false;
            IsBusy = false;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Files.Clear();
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent(GetTranslation("Common_NotConnected"))
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
            return;
        }

        IsDeviceConnected = true;
        UpdateQuickAccessPathsByDeviceState();

        var device = Global.DeviceManager?.Devices.FirstOrDefault(d => d.Id == Global.thisdevice);
        if (device == null || (device.Transport != TransportType.Adb && device.Transport != TransportType.Hdc))
        {
            IsBusy = false;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent(GetTranslation("Common_ModeError"))
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
            return;
        }

        // Clear existing items on UI thread first to avoid virtualization recycling conflicts
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Files.Clear();
            SelectedCount = 0;
        });

        try
        {
            // Ensure trailing slash so symlinks like /sdcard are followed into the directory
            string listPath = path.EndsWith("/") ? path : path + "/";
            string output = await RunADB($"ls -la \"{listPath}\"");

            if (output.Contains("No such file", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("Permission denied", StringComparison.OrdinalIgnoreCase) ||
                output.Contains("opendir failed", StringComparison.OrdinalIgnoreCase))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Error)
                        .WithTitle(GetTranslation("Common_Error"))
                        .WithContent($"[LoadDirectory] Path: {path}\n\n{output.Trim()}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                });
                return;
            }

            var parsedFiles = AdbFileListParser.Parse(output, path);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (var file in parsedFiles)
                {
                    Files.Add(file);
                }
            });

            CurrentPath = path;
            PathInput = path;
            HasLoaded = true;
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[LoadDirectory] Path: {path}\n\n{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// <para>导航到当前目录的父目录。</para>
    /// Navigates to the parent directory of the current path.
    /// </summary>
    [RelayCommand]
    private async Task NavigateUpAsync()
    {
        if (string.IsNullOrEmpty(CurrentPath) || CurrentPath == "/")
            return;

        string parentPath;
        int lastSlash = CurrentPath.LastIndexOf('/');

        if (lastSlash <= 0)
        {
            parentPath = "/";
        }
        else
        {
            parentPath = CurrentPath[..lastSlash];
        }

        await LoadDirectoryAsync(parentPath);
    }

    /// <summary>
    /// <para>刷新当前目录内容。</para>
    /// Refreshes the current directory contents.
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDirectoryAsync(CurrentPath);
    }

    /// <summary>
    /// <para>打开指定的文件条目。若为目录则导航进入，否则不执行操作。</para>
    /// Opens the specified file entry. Navigates into it if it is a directory; otherwise does nothing.
    /// </summary>
    /// <param name="entry">
    /// <para>要打开的文件条目。</para>
    /// The file entry to open.
    /// </param>
    [RelayCommand]
    private async Task OpenFileEntryAsync(FileEntry entry)
    {
        if (entry is null)
            return;

        SelectedFile = entry;

        if (entry.IsDirectory)
        {
            try
            {
                await LoadDirectoryAsync(entry.FullPath);
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Error)
                        .WithTitle(GetTranslation("Common_Error"))
                        .WithContent($"[OpenFileEntry] Name: {entry.Name}, Path: {entry.FullPath}\n\n{ex.GetType().Name}: {ex.Message}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                });
            }
        }
        else
        {
            // Double-click on a file: pull to temp directory and open with system default program
            if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;
            IsBusy = true;
            try
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "UotanToolbox_FileOpen");
                Directory.CreateDirectory(tempDir);
                var localPath = Path.Combine(tempDir, entry.Name);

                string output = await RunADB($"pull \"{entry.FullPath}\" \"{localPath}\"", false);

                if (System.IO.File.Exists(localPath))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = localPath,
                            UseShellExecute = true,
                            Verb = "open"
                        });
                    }
                    catch (Exception openEx)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Global.MainDialogManager.CreateDialog()
                                .OfType(NotificationType.Error)
                                .WithTitle(GetTranslation("Common_Error"))
                                .WithContent($"[OpenFile] Name: {entry.Name}\n\n{openEx.GetType().Name}: {openEx.Message}")
                                .Dismiss().ByClickingBackground()
                                .TryShow();
                        });
                    }
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Error)
                            .WithTitle(GetTranslation("Filemgr_PullFailed"))
                            .WithContent($"[OpenFile] Name: {entry.Name}, Path: {entry.FullPath}\n\n{output.Trim()}")
                            .Dismiss().ByClickingBackground()
                            .TryShow();
                    });
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Error)
                        .WithTitle(GetTranslation("Common_Error"))
                        .WithContent($"[OpenFile] Name: {entry.Name}, Path: {entry.FullPath}\n\n{ex.GetType().Name}: {ex.Message}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                });
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    /// <summary>
    /// <para>根据路径输入框中的文本导航到指定目录。</para>
    /// Navigates to the directory specified in the path input box.
    /// </summary>
    [RelayCommand]
    private async Task NavigateToPathAsync()
    {
        if (!string.IsNullOrWhiteSpace(PathInput))
        {
            try
            {
                await LoadDirectoryAsync(PathInput.Trim());
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Error)
                        .WithTitle(GetTranslation("Common_Error"))
                        .WithContent($"[NavigateToPath] Path: {PathInput.Trim()}\n\n{ex.GetType().Name}: {ex.Message}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                });
            }
        }
    }

    /// <summary>
    /// <para>导航到快捷访问项指定的目录。</para>
    /// Navigates to the directory specified by the quick access item.
    /// </summary>
    /// <param name="item">
    /// <para>快捷访问项。</para>
    /// The quick access item.
    /// </param>
    [RelayCommand]
    private async Task NavigateToQuickAccessAsync(QuickAccessItem item)
    {
        if (item is not null)
        {
            try
            {
                await LoadDirectoryAsync(item.Path);
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Error)
                        .WithTitle(GetTranslation("Common_Error"))
                        .WithContent($"[NavigateToQuickAccess] Name: {item.Name}, Path: {item.Path}\n\n{ex.GetType().Name}: {ex.Message}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                });
            }
        }
    }

    /// <summary>
    /// <para>将本地文件推送到设备当前目录。弹出文件选择对话框让用户选择文件，然后执行 ADB push 命令。</para>
    /// Pushes a local file to the current device directory. Shows a file picker dialog, then executes the ADB push command.
    /// </summary>
    [RelayCommand]
    private async Task PushFileAsync()
    {
        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;

        try
        {
            var mainWindow = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow is null)
                return;

            System.Collections.Generic.IReadOnlyList<IStorageFile> files = await mainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = GetTranslation("Filemgr_Import"),
                AllowMultiple = true
            });

            if (files.Count < 1)
                return;

            IsBusy = true;
            try
            {
                foreach (var file in files)
                {
                    string localPath = file.TryGetLocalPath() ?? string.Empty;
                    if (string.IsNullOrEmpty(localPath))
                        continue;

                    string remotePath = CurrentPath == "/"
                        ? $"/{Path.GetFileName(localPath)}"
                        : $"{CurrentPath}/{Path.GetFileName(localPath)}";

                    string output = await RunADB($"push \"{localPath}\" \"{remotePath}\"", false);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (output.Contains("bytes in", StringComparison.OrdinalIgnoreCase) ||
                            output.Contains("file pushed", StringComparison.OrdinalIgnoreCase) ||
                            output.Contains("TransferSummary success", StringComparison.OrdinalIgnoreCase) ||
                            output.Contains("FileTransfer finish", StringComparison.OrdinalIgnoreCase) ||
                            (!output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                             !output.Contains("failed", StringComparison.OrdinalIgnoreCase)))
                        {
                            Global.MainToastManager.CreateToast()
                                .WithTitle(GetTranslation("Filemgr_PushSuccess"))
                                .WithContent(Path.GetFileName(localPath))
                                .OfType(NotificationType.Success)
                                .Dismiss().ByClicking()
                                .Dismiss().After(TimeSpan.FromSeconds(3))
                                .Queue();
                        }
                        else
                        {
                            Global.MainDialogManager.CreateDialog()
                                .OfType(NotificationType.Error)
                                .WithTitle(GetTranslation("Filemgr_PushFailed"))
                                .WithContent($"[PushFile] Local: {localPath}, Remote: {remotePath}\n\n{output.Trim()}")
                                .Dismiss().ByClickingBackground()
                                .TryShow();
                        }
                    });
                }

                await RefreshAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }
        catch (Exception ex)
        {
            IsBusy = false;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[PushFile]\n\n{ex.GetType().Name}: {ex.Message}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
    }

    /// <summary>
    /// <para>将本地文件夹推送到设备当前目录。弹出文件夹选择对话框让用户选择文件夹，然后执行 ADB push 命令。</para>
    /// Pushes a local folder to the current device directory. Shows a folder picker dialog, then executes the ADB push command.
    /// </summary>
    [RelayCommand]
    private async Task PushFolderAsync()
    {
        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;

        try
        {
            var mainWindow = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow is null)
                return;

            System.Collections.Generic.IReadOnlyList<IStorageFolder> folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = GetTranslation("Filemgr_Import"),
                AllowMultiple = false
            });

            if (folders.Count < 1)
                return;

            string localPath = folders[0].TryGetLocalPath() ?? string.Empty;
            if (string.IsNullOrEmpty(localPath))
                return;

            string folderName = Path.GetFileName(localPath);
            string remotePath = CurrentPath == "/"
                ? $"/{folderName}"
                : $"{CurrentPath}/{folderName}";

            IsBusy = true;
            try
            {
                string output = await RunADB($"push \"{localPath}\" \"{remotePath}\"", false);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (output.Contains("bytes in", StringComparison.OrdinalIgnoreCase) ||
                        output.Contains("file pushed", StringComparison.OrdinalIgnoreCase) ||
                        output.Contains("files pushed", StringComparison.OrdinalIgnoreCase) ||
                        output.Contains("TransferSummary success", StringComparison.OrdinalIgnoreCase) ||
                        output.Contains("FileTransfer finish", StringComparison.OrdinalIgnoreCase) ||
                        (!output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                         !output.Contains("failed", StringComparison.OrdinalIgnoreCase)))
                    {
                        Global.MainToastManager.CreateToast()
                            .WithTitle(GetTranslation("Filemgr_PushSuccess"))
                            .WithContent(folderName)
                            .OfType(NotificationType.Success)
                            .Dismiss().ByClicking()
                            .Dismiss().After(TimeSpan.FromSeconds(3))
                            .Queue();
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Error)
                            .WithTitle(GetTranslation("Filemgr_PushFailed"))
                            .WithContent($"[PushFolder] Local: {localPath}, Remote: {remotePath}\n\n{output.Trim()}")
                            .Dismiss().ByClickingBackground()
                            .TryShow();
                    }
                });

                await RefreshAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }
        catch (Exception ex)
        {
            IsBusy = false;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[PushFolder]\n\n{ex.GetType().Name}: {ex.Message}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
    }

    /// <summary>
    /// <para>将设备上的文件或目录拉取到本地。若有选中项则批量拉取，否则拉取单个条目。</para>
    /// Pulls files or directories from the device to local. Batch pulls if items are selected, otherwise pulls a single entry.
    /// </summary>
    /// <param name="entry">
    /// <para>要拉取的单个文件条目（无选中项时使用）。</para>
    /// The single file entry to pull (used when no items are selected).
    /// </param>
    [RelayCommand]
    private async Task PullFileAsync(FileEntry? entry)
    {
        var selected = Files.Where(f => f.IsSelected).ToList();
        if (selected.Count > 0)
        {
            await PullFilesBatchAsync(selected);
            return;
        }

        if (entry is null)
            return;

        await PullSingleFileAsync(entry);
    }

    /// <summary>
    /// <para>批量拉取选中的文件或目录到本地。</para>
    /// Batch pulls selected files or directories to local.
    /// </summary>
    private async Task PullFilesBatchAsync(List<FileEntry> entries)
    {
        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;

        try
        {
            var mainWindow = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow is null)
                return;

            System.Collections.Generic.IReadOnlyList<IStorageFolder> folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = GetTranslation("Filemgr_SelectSaveLocation"),
                AllowMultiple = false
            });
            if (folders.Count < 1)
                return;
            string localPath = folders[0].TryGetLocalPath() ?? string.Empty;
            if (string.IsNullOrEmpty(localPath))
                return;

            IsBusy = true;
            try
            {
                int successCount = 0;
                int failCount = 0;

                foreach (var e in entries)
                {
                    string output = await RunADB($"pull \"{e.FullPath}\" \"{localPath}\"", false);

                    if (output.Contains("bytes in", StringComparison.OrdinalIgnoreCase) ||
                        output.Contains("file pulled", StringComparison.OrdinalIgnoreCase) ||
                        output.Contains("TransferSummary success", StringComparison.OrdinalIgnoreCase) ||
                        output.Contains("FileTransfer finish", StringComparison.OrdinalIgnoreCase) ||
                        (!output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                         !output.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
                         !output.Contains("No such file", StringComparison.OrdinalIgnoreCase)))
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (failCount == 0)
                    {
                        Global.MainToastManager.CreateToast()
                            .WithTitle(GetTranslation("Filemgr_PullSuccess"))
                            .WithContent($"{successCount}")
                            .OfType(NotificationType.Success)
                            .Dismiss().ByClicking()
                            .Dismiss().After(TimeSpan.FromSeconds(3))
                            .Queue();
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Warning)
                            .WithTitle(GetTranslation("Filemgr_PullFailed"))
                            .WithContent($"Success: {successCount}, Failed: {failCount}")
                            .Dismiss().ByClickingBackground()
                            .TryShow();
                    }
                });
            }
            finally
            {
                IsBusy = false;
            }
        }
        catch (Exception ex)
        {
            IsBusy = false;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[PullFilesBatch]\n\n{ex.GetType().Name}: {ex.Message}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
    }

    /// <summary>
    /// <para>拉取单个文件或目录到本地。</para>
    /// Pulls a single file or directory to local.
    /// </summary>
    private async Task PullSingleFileAsync(FileEntry entry)
    {
        if (entry is null)
            return;

        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;

        try
        {
            var mainWindow = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow is null)
                return;

            System.Collections.Generic.IReadOnlyList<IStorageFolder> folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = GetTranslation("Filemgr_SelectSaveLocation"),
                AllowMultiple = false
            });
            if (folders.Count < 1)
                return;
            string localPath = folders[0].TryGetLocalPath() ?? string.Empty;
            if (string.IsNullOrEmpty(localPath))
                return;

            IsBusy = true;
            try
            {
                string output = await RunADB($"pull \"{entry.FullPath}\" \"{localPath}\"", false);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (output.Contains("bytes in", StringComparison.OrdinalIgnoreCase) ||
                        output.Contains("file pulled", StringComparison.OrdinalIgnoreCase) ||
                        output.Contains("TransferSummary success", StringComparison.OrdinalIgnoreCase) ||
                        output.Contains("FileTransfer finish", StringComparison.OrdinalIgnoreCase) ||
                        (!output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                         !output.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
                         !output.Contains("No such file", StringComparison.OrdinalIgnoreCase)))
                    {
                        Global.MainToastManager.CreateToast()
                            .WithTitle(GetTranslation("Filemgr_PullSuccess"))
                            .WithContent(entry.Name)
                            .OfType(NotificationType.Success)
                            .Dismiss().ByClicking()
                            .Dismiss().After(TimeSpan.FromSeconds(3))
                            .Queue();
                    }
                    else
                    {
                        Global.MainDialogManager.CreateDialog()
                            .OfType(NotificationType.Error)
                            .WithTitle(GetTranslation("Filemgr_PullFailed"))
                            .WithContent($"[PullFile] Name: {entry.Name}, Path: {entry.FullPath}\n\n{output.Trim()}")
                            .Dismiss().ByClickingBackground()
                            .TryShow();
                    }
                });
            }
            finally
            {
                IsBusy = false;
            }
        }
        catch (Exception ex)
        {
            IsBusy = false;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[PullFile] Name: {entry.Name}, Path: {entry.FullPath}\n\n{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
    }

    private async Task ModifyPermissionsInternalAsync(FileEntry? entry, string mode)
    {
        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;

        var selected = Files.Where(f => f.IsSelected).ToList();
        var targetEntries = selected.Count > 0 ? selected : (entry != null ? new List<FileEntry> { entry } : new List<FileEntry>());

        if (targetEntries.Count == 0) return;

        try
        {
            foreach (var target in targetEntries)
            {
                await RunADB($"chmod {mode} \"{target.FullPath}\"");
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainToastManager.CreateToast()
                    .WithTitle(GetTranslation("Filemgr_ModifyPermissions"))
                    .WithContent(mode)
                    .OfType(NotificationType.Information)
                    .Dismiss().ByClicking()
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
            });

            _ = RefreshAsync();
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[ModifyPermissions]\n\n{ex.GetType().Name}: {ex.Message}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
    }

    [RelayCommand]
    private async Task ModifyPermissionsToX(FileEntry? entry)
    {
        await ModifyPermissionsInternalAsync(entry, "+x");
    }

    [RelayCommand]
    private async Task ModifyPermissionsTo644(FileEntry? entry)
    {
        await ModifyPermissionsInternalAsync(entry, "644");
    }

    [RelayCommand]
    private async Task ModifyPermissionsTo755(FileEntry? entry)
    {
        await ModifyPermissionsInternalAsync(entry, "755");
    }

    [RelayCommand]
    private async Task ModifyPermissionsTo777(FileEntry? entry)
    {
        await ModifyPermissionsInternalAsync(entry, "777");
    }

    [RelayCommand]
    private async Task ModifyPermissionsToCustom(FileEntry? entry)
    {
        string? mode = await ShowInputDialogAsync(GetTranslation("Filemgr_CustomPermissions"), GetTranslation("Filemgr_NoChmod"), "");
        if (!string.IsNullOrEmpty(mode))
        {
            await ModifyPermissionsInternalAsync(entry, mode);
        }
    }

    /// <summary>
    /// <para>将设备上的文件拉取到本地桌面目录。若有选中项则批量拉取，否则拉取单个条目。</para>
    /// Pulls files from the device to the local Desktop directory. Batch pulls if items are selected, otherwise pulls a single entry.
    /// </summary>
    /// <param name="entry">
    /// <para>要拉取的单个文件条目（无选中项时使用）。</para>
    /// The single file entry to pull (used when no items are selected).
    /// </param>
    [RelayCommand]
    private async Task ExtractToDesktopAsync(FileEntry? entry)
    {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var selected = Files.Where(f => f.IsSelected).ToList();
        if (selected.Count > 0)
        {
            await ExtractEntriesToDirectoryAsync(selected, desktopPath);
            return;
        }

        if (entry is null)
            return;
        await ExtractToDirectoryAsync(entry, desktopPath);
    }

    /// <summary>
    /// <para>将设备上的文件拉取到本地下载目录。若有选中项则批量拉取，否则拉取单个条目。</para>
    /// Pulls files from the device to the local Downloads directory. Batch pulls if items are selected, otherwise pulls a single entry.
    /// </summary>
    /// <param name="entry">
    /// <para>要拉取的单个文件条目（无选中项时使用）。</para>
    /// The single file entry to pull (used when no items are selected).
    /// </param>
    [RelayCommand]
    private async Task ExtractToDownloadsAsync(FileEntry? entry)
    {
        string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string downloadsDir = Path.Combine(downloadsPath, "Downloads");
        if (!Directory.Exists(downloadsDir))
            Directory.CreateDirectory(downloadsDir);

        var selected = Files.Where(f => f.IsSelected).ToList();
        if (selected.Count > 0)
        {
            await ExtractEntriesToDirectoryAsync(selected, downloadsDir);
            return;
        }

        if (entry is null)
            return;
        await ExtractToDirectoryAsync(entry, downloadsDir);
    }

    /// <summary>
    /// <para>将设备上的文件拉取到用户选择的自定义目录。若有选中项则批量拉取，否则拉取单个条目。</para>
    /// Pulls files from the device to a user-selected custom directory. Batch pulls if items are selected, otherwise pulls a single entry.
    /// </summary>
    /// <param name="entry">
    /// <para>要拉取的单个文件条目（无选中项时使用）。</para>
    /// The single file entry to pull (used when no items are selected).
    /// </param>
    [RelayCommand]
    private async Task ExtractToCustomDirAsync(FileEntry? entry)
    {
        var selected = Files.Where(f => f.IsSelected).ToList();
        if (selected.Count > 0)
        {
            await ExtractEntriesToCustomDirAsync(selected);
            return;
        }

        if (entry is null)
            return;
        await ExtractSingleToCustomDirAsync(entry);
    }

    /// <summary>
    /// <para>将设备上的文件拉取到指定的本地目录。</para>
    /// Pulls a file from the device to the specified local directory.
    /// </summary>
    /// <param name="entry">
    /// <para>要拉取的文件条目。</para>
    /// The file entry to pull.
    /// </param>
    /// <param name="localDir">
    /// <para>本地目标目录路径。</para>
    /// The local target directory path.
    /// </param>
    private async Task ExtractToDirectoryAsync(FileEntry entry, string localDir)
    {
        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;
        IsBusy = true;
        try
        {
            string output = await RunADB($"pull \"{entry.FullPath}\" \"{localDir}\"", false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (output.Contains("bytes in", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("file pulled", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("TransferSummary success", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("FileTransfer finish", StringComparison.OrdinalIgnoreCase) ||
                    (!output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                     !output.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
                     !output.Contains("No such file", StringComparison.OrdinalIgnoreCase)))
                {
                    Global.MainToastManager.CreateToast()
                        .WithTitle(GetTranslation("Filemgr_PullSuccess"))
                        .WithContent($"{entry.Name} -> {localDir}")
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Error)
                        .WithTitle(GetTranslation("Filemgr_PullFailed"))
                        .WithContent($"[ExtractTo] Name: {entry.Name}, Target: {localDir}\n\n{output.Trim()}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                }
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[ExtractTo] Name: {entry.Name}\n\n{ex.GetType().Name}: {ex.Message}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// <para>批量将选中的文件条目拉取到指定的本地目录。</para>
    /// Batch pulls selected file entries to the specified local directory.
    /// </summary>
    private async Task ExtractEntriesToDirectoryAsync(List<FileEntry> entries, string localDir)
    {
        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;
        IsBusy = true;
        try
        {
            int successCount = 0;
            int failCount = 0;

            foreach (var e in entries)
            {
                string output = await RunADB($"pull \"{e.FullPath}\" \"{localDir}\"", false);

                if (output.Contains("bytes in", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("file pulled", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("TransferSummary success", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("FileTransfer finish", StringComparison.OrdinalIgnoreCase) ||
                    (!output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                     !output.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
                     !output.Contains("No such file", StringComparison.OrdinalIgnoreCase)))
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (failCount == 0)
                {
                    Global.MainToastManager.CreateToast()
                        .WithTitle(GetTranslation("Filemgr_PullSuccess"))
                        .WithContent($"{successCount}")
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Warning)
                        .WithTitle(GetTranslation("Filemgr_PullFailed"))
                        .WithContent($"Success: {successCount}, Failed: {failCount}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                }
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[ExtractEntriesBatch]\n\n{ex.GetType().Name}: {ex.Message}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// <para>将单个文件条目拉取到用户选择的自定义目录。</para>
    /// Pulls a single file entry to a user-selected custom directory.
    /// </summary>
    private async Task ExtractSingleToCustomDirAsync(FileEntry entry)
    {
        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;

        try
        {
            var mainWindow = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow is null)
                return;

            System.Collections.Generic.IReadOnlyList<IStorageFolder> folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = GetTranslation("Filemgr_SelectSaveLocation"),
                AllowMultiple = false
            });
            if (folders.Count < 1)
                return;
            string localPath = folders[0].TryGetLocalPath() ?? string.Empty;
            if (string.IsNullOrEmpty(localPath))
                return;

            await ExtractToDirectoryAsync(entry, localPath);
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[ExtractToCustomDir] Name: {entry.Name}\n\n{ex.GetType().Name}: {ex.Message}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
    }

    /// <summary>
    /// <para>批量将选中的文件条目拉取到用户选择的自定义目录。</para>
    /// Batch pulls selected file entries to a user-selected custom directory.
    /// </summary>
    private async Task ExtractEntriesToCustomDirAsync(List<FileEntry> entries)
    {
        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;

        try
        {
            var mainWindow = (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow is null)
                return;

            System.Collections.Generic.IReadOnlyList<IStorageFolder> folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = GetTranslation("Filemgr_SelectSaveLocation"),
                AllowMultiple = false
            });
            if (folders.Count < 1)
                return;
            string localPath = folders[0].TryGetLocalPath() ?? string.Empty;
            if (string.IsNullOrEmpty(localPath))
                return;

            await ExtractEntriesToDirectoryAsync(entries, localPath);
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[ExtractEntriesToCustomDir]\n\n{ex.GetType().Name}: {ex.Message}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
    }

    /// <summary>
    /// <para>在设备当前目录下创建新文件。弹出输入对话框获取文件名，然后执行 ADB shell touch 命令。</para>
    /// Creates a new file in the current device directory. Shows an input dialog to get the file name, then executes the ADB shell touch command.
    /// </summary>
    [RelayCommand]
    private async Task NewFileAsync()
    {
        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;

        try
        {
            string? fileName = await ShowInputDialogAsync(GetTranslation("Filemgr_NewFileTitle"), GetTranslation("Filemgr_EnterFileName"), "");
            if (string.IsNullOrEmpty(fileName))
                return;

            if (!IsValidFileName(fileName))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Global.MainToastManager.CreateToast()
                        .WithTitle(GetTranslation("Filemgr_InvalidName"))
                        .WithContent(fileName)
                        .OfType(NotificationType.Error)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                });
                return;
            }

            string remotePath = CurrentPath == "/" ? $"/{fileName}" : $"{CurrentPath}/{fileName}";
            string output = await RunADB($"touch \"{remotePath}\"");

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("Permission denied", StringComparison.OrdinalIgnoreCase))
                {
                    Global.MainToastManager.CreateToast()
                        .WithTitle(GetTranslation("Filemgr_NewFileSuccess"))
                        .WithContent(fileName)
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                    _ = RefreshAsync();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Error)
                        .WithTitle(GetTranslation("Filemgr_NewFileFailed"))
                        .WithContent($"[NewFile] Name: {fileName}, Path: {remotePath}\n\n{output.Trim()}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                }
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[NewFile]\n\n{ex.GetType().Name}: {ex.Message}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
    }

    /// <summary>
    /// <para>在设备当前目录下创建新文件夹。弹出输入对话框获取文件夹名，然后执行 ADB shell mkdir 命令。</para>
    /// Creates a new folder in the current device directory. Shows an input dialog to get the folder name, then executes the ADB shell mkdir command.
    /// </summary>
    [RelayCommand]
    private async Task NewFolderAsync()
    {
        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;

        try
        {
            string? folderName = await ShowInputDialogAsync(GetTranslation("Filemgr_NewFolderTitle"), GetTranslation("Filemgr_EnterFolderName"), "");
            if (string.IsNullOrEmpty(folderName))
                return;

            if (!IsValidFileName(folderName))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Global.MainToastManager.CreateToast()
                        .WithTitle(GetTranslation("Filemgr_InvalidName"))
                        .WithContent(folderName)
                        .OfType(NotificationType.Error)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                });
                return;
            }

            string remotePath = CurrentPath == "/" ? $"/{folderName}" : $"{CurrentPath}/{folderName}";
            string output = await RunADB($"mkdir \"{remotePath}\"");

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("Permission denied", StringComparison.OrdinalIgnoreCase))
                {
                    Global.MainToastManager.CreateToast()
                        .WithTitle(GetTranslation("Filemgr_NewFolderSuccess"))
                        .WithContent(folderName)
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                    _ = RefreshAsync();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Error)
                        .WithTitle(GetTranslation("Filemgr_NewFolderFailed"))
                        .WithContent($"[NewFolder] Name: {folderName}, Path: {remotePath}\n\n{output.Trim()}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                }
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[NewFolder]\n\n{ex.GetType().Name}: {ex.Message}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
    }

    /// <summary>
    /// <para>将文件条目复制到剪贴板，标记为复制操作。若有选中项则批量复制，否则复制单个条目。</para>
    /// Copies file entries to the clipboard, marking as a copy operation. Batch copies if items are selected, otherwise copies a single entry.
    /// </summary>
    /// <param name="entry">
    /// <para>要复制的单个文件条目（无选中项时使用）。</para>
    /// The single file entry to copy (used when no items are selected).
    /// </param>
    [RelayCommand]
    private void CopyFile(FileEntry? entry)
    {
        var selected = Files.Where(f => f.IsSelected).ToList();
        if (selected.Count > 0)
        {
            _clipboardEntries = [.. selected];
        }
        else if (entry is not null)
        {
            SelectedFile = entry;
            _clipboardEntries = [entry];
        }
        else
        {
            return;
        }

        _clipboardOperation = ClipboardOperation.Copy;
        CanPaste = true;
    }

    /// <summary>
    /// <para>将文件条目剪切到剪贴板，标记为剪切操作。若有选中项则批量剪切，否则剪切单个条目。</para>
    /// Cuts file entries to the clipboard, marking as a cut operation. Batch cuts if items are selected, otherwise cuts a single entry.
    /// </summary>
    /// <param name="entry">
    /// <para>要剪切的单个文件条目（无选中项时使用）。</para>
    /// The single file entry to cut (used when no items are selected).
    /// </param>
    [RelayCommand]
    private void CutFile(FileEntry? entry)
    {
        var selected = Files.Where(f => f.IsSelected).ToList();
        if (selected.Count > 0)
        {
            _clipboardEntries = [.. selected];
        }
        else if (entry is not null)
        {
            SelectedFile = entry;
            _clipboardEntries = [entry];
        }
        else
        {
            return;
        }

        _clipboardOperation = ClipboardOperation.Cut;
        CanPaste = true;
    }

    /// <summary>
    /// <para>将剪贴板中的文件条目粘贴到当前目录。支持批量粘贴，根据剪贴板操作类型执行复制或移动命令。</para>
    /// Pastes file entries from the clipboard to the current directory. Supports batch paste, executing copy or move commands based on the clipboard operation type.
    /// </summary>
    [RelayCommand]
    private async Task PasteFileAsync()
    {
        if (_clipboardEntries.Count == 0 || _clipboardOperation == ClipboardOperation.None)
            return;

        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;
        IsBusy = true;

        try
        {
            int successCount = 0;
            int failCount = 0;

            foreach (var clipboardEntry in _clipboardEntries)
            {
                string destPath = CurrentPath == "/"
                    ? $"/{clipboardEntry.Name}"
                    : $"{CurrentPath}/{clipboardEntry.Name}";
                string output;

                if (_clipboardOperation == ClipboardOperation.Copy)
                {
                    string cmd = clipboardEntry.IsDirectory
                        ? $"cp -r \"{clipboardEntry.FullPath}\" \"{destPath}\""
                        : $"cp \"{clipboardEntry.FullPath}\" \"{destPath}\"";
                    output = await RunADB(cmd);
                }
                else // Cut
                {
                    output = await RunADB($"mv \"{clipboardEntry.FullPath}\" \"{destPath}\"");
                }

                if (!output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("Permission denied", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("No such file", StringComparison.OrdinalIgnoreCase))
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (failCount == 0)
                {
                    Global.MainToastManager.CreateToast()
                        .WithTitle(GetTranslation("Filemgr_PasteSuccess"))
                        .WithContent($"{successCount}")
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Warning)
                        .WithTitle(GetTranslation("Filemgr_PasteFailed"))
                        .WithContent($"Success: {successCount}, Failed: {failCount}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                }
            });

            // If cut operation, clear clipboard after successful move
            if (_clipboardOperation == ClipboardOperation.Cut)
            {
                _clipboardEntries = [];
                _clipboardOperation = ClipboardOperation.None;
                CanPaste = false;
            }

            _ = RefreshAsync();
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[Paste]\n\n{ex.GetType().Name}: {ex.Message}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// <para>重命名设备上的文件或目录。弹出输入对话框获取新名称，验证后执行 ADB shell mv 命令。</para>
    /// Renames a file or directory on the device. Shows an input dialog to get the new name, validates it, then executes the ADB shell mv command.
    /// </summary>
    /// <param name="entry">
    /// <para>要重命名的文件条目。</para>
    /// The file entry to rename.
    /// </param>
    [RelayCommand]
    private async Task RenameFileAsync(FileEntry entry)
    {
        if (entry is null)
            return;

        SelectedFile = entry;

        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;

        try
        {
            string? newName = await ShowInputDialogAsync(GetTranslation("Filemgr_RenameTitle"), GetTranslation("Filemgr_EnterNewName"), entry.Name);
            if (string.IsNullOrEmpty(newName))
                return;

            if (!IsValidFileName(newName))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Global.MainToastManager.CreateToast()
                        .WithTitle(GetTranslation("Filemgr_InvalidName"))
                        .WithContent(newName)
                        .OfType(NotificationType.Error)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                });
                return;
            }

            string parentPath = GetParentPath(entry.FullPath);
            string newFullPath = parentPath == "/" ? $"/{newName}" : $"{parentPath}/{newName}";
            string output = await RunADB($"mv \"{entry.FullPath}\" \"{newFullPath}\"");

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("No such file", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("Permission denied", StringComparison.OrdinalIgnoreCase))
                {
                    Global.MainToastManager.CreateToast()
                        .WithTitle(GetTranslation("Filemgr_RenameSuccess"))
                        .WithContent(newName)
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                    _ = RefreshAsync();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Error)
                        .WithTitle(GetTranslation("Filemgr_RenameFailed"))
                        .WithContent($"[Rename] Name: {entry.Name}, Path: {entry.FullPath}\n\n{output.Trim()}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                }
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[Rename] Name: {entry.Name}, Path: {entry.FullPath}\n\n{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
    }

    /// <summary>
    /// <para>删除设备上的文件或目录。弹出确认对话框后执行 ADB shell rm 命令。</para>
    /// Deletes a file or directory on the device. Shows a confirmation dialog, then executes the ADB shell rm command.
    /// </summary>
    /// <param name="entry">
    /// <para>要删除的文件条目。</para>
    /// The file entry to delete.
    /// </param>
    [RelayCommand]
    private async Task DeleteFileAsync(FileEntry entry)
    {
        if (entry is null)
            return;

        SelectedFile = entry;

        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;

        try
        {
            var tcs = new TaskCompletionSource<bool>();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Filemgr_ConfirmDeleteTitle"))
                    .WithContent(string.Format(GetTranslation("Filemgr_ConfirmDelete"), entry.Name))
                    .WithActionButton(GetTranslation("Filemgr_Confirm"), _ => tcs.TrySetResult(true), true)
                    .WithActionButton(GetTranslation("Filemgr_Cancel"), _ => tcs.TrySetResult(false), true)
                    .TryShow();
            });

            bool confirmed = await tcs.Task;
            if (!confirmed)
                return;

            string cmd = entry.IsDirectory
                ? $"rm -rf \"{entry.FullPath}\""
                : $"rm \"{entry.FullPath}\"";
            string output = await RunADB(cmd);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("No such file", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("Permission denied", StringComparison.OrdinalIgnoreCase))
                {
                    Global.MainToastManager.CreateToast()
                        .WithTitle(GetTranslation("Filemgr_DeleteSuccess"))
                        .WithContent(entry.Name)
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                    _ = RefreshAsync();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Error)
                        .WithTitle(GetTranslation("Filemgr_DeleteFailed"))
                        .WithContent($"[Delete] Name: {entry.Name}, Path: {entry.FullPath}\n\n{output.Trim()}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                }
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[Delete] Name: {entry.Name}, Path: {entry.FullPath}\n\n{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
    }
    
    /// <summary>
    /// <para>批量删除选中的文件或目录。弹出确认对话框后逐个执行 ADB shell rm 命令。</para>
    /// Batch deletes selected files or directories. Shows a confirmation dialog, then executes ADB shell rm commands one by one.
    /// </summary>
    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        var selected = Files.Where(f => f.IsSelected).ToList();
        if (selected.Count == 0)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainToastManager.CreateToast()
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent(GetTranslation("Filemgr_NoSelection"))
                    .OfType(NotificationType.Information)
                    .Dismiss().ByClicking()
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
            });
            return;
        }

        if (!await EnsureDeviceConnectedAndRefreshStateAsync()) return;

        try
        {
            var tcs = new TaskCompletionSource<bool>();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Filemgr_ConfirmDeleteTitle"))
                    .WithContent(string.Format(GetTranslation("Filemgr_ConfirmDeleteCount"), selected.Count))
                    .WithActionButton(GetTranslation("Filemgr_Confirm"), _ => tcs.TrySetResult(true), true)
                    .WithActionButton(GetTranslation("Filemgr_Cancel"), _ => tcs.TrySetResult(false), true)
                    .TryShow();
            });

            bool confirmed = await tcs.Task;
            if (!confirmed)
                return;

            IsBusy = true;
            int successCount = 0;
            int failCount = 0;

            foreach (var entry in selected)
            {
                string cmd = entry.IsDirectory
                    ? $"rm -rf \"{entry.FullPath}\""
                    : $"rm \"{entry.FullPath}\"";
                string output = await RunADB(cmd);

                if (!output.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("No such file", StringComparison.OrdinalIgnoreCase) &&
                    !output.Contains("Permission denied", StringComparison.OrdinalIgnoreCase))
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (failCount == 0)
                {
                    Global.MainToastManager.CreateToast()
                        .WithTitle(GetTranslation("Filemgr_DeleteSuccess"))
                        .WithContent($"{successCount}")
                        .OfType(NotificationType.Success)
                        .Dismiss().ByClicking()
                        .Dismiss().After(TimeSpan.FromSeconds(3))
                        .Queue();
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                        .OfType(NotificationType.Warning)
                        .WithTitle(GetTranslation("Filemgr_DeleteFailed"))
                        .WithContent($"Success: {successCount}, Failed: {failCount}")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                }
            });

            _ = RefreshAsync();
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .OfType(NotificationType.Error)
                    .WithTitle(GetTranslation("Common_Error"))
                    .WithContent($"[DeleteSelected]\n\n{ex.GetType().Name}: {ex.Message}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// <para>将指定的目录条目添加到快捷访问列表并持久化。</para>
    /// Adds the specified directory entry to the quick access list and persists it.
    /// </summary>
    /// <param name="entry">
    /// <para>要添加到快捷访问的文件条目（须为目录）。</para>
    /// The file entry to add to quick access (must be a directory).
    /// </param>
    [RelayCommand]
    private async Task AddToQuickAccessAsync(FileEntry entry)
    {
        if (entry is null || !entry.IsDirectory)
            return;

        // Check if already in quick access
        if (QuickAccessItems.Any(i => string.Equals(i.Path, entry.FullPath, StringComparison.OrdinalIgnoreCase)))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainToastManager.CreateToast()
                    .WithTitle(GetTranslation("Filemgr_QuickAccessAlreadyExists"))
                    .WithContent(entry.Name)
                    .OfType(NotificationType.Information)
                    .Dismiss().ByClicking()
                    .Dismiss().After(TimeSpan.FromSeconds(3))
                    .Queue();
            });
            return;
        }

        var newItem = new QuickAccessItem
        {
            Name = entry.Name,
            Path = entry.FullPath,
            Icon = MaterialIconKind.Pin,
            IsCustom = true
        };

        QuickAccessItems.Add(newItem);
        SaveCustomQuickAccessItems();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Global.MainToastManager.CreateToast()
                .WithTitle(GetTranslation("Filemgr_QuickAccessAdded"))
                .WithContent(entry.Name)
                .OfType(NotificationType.Success)
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
        });
    }

    /// <summary>
    /// <para>从快捷访问列表中移除指定的自定义快捷访问项并持久化。</para>
    /// Removes the specified custom quick access item from the list and persists the change.
    /// </summary>
    /// <param name="item">
    /// <para>要移除的快捷访问项。</para>
    /// The quick access item to remove.
    /// </param>
    [RelayCommand]
    private async Task RemoveFromQuickAccessAsync(QuickAccessItem item)
    {
        if (item is null || !item.IsCustom)
            return;

        QuickAccessItems.Remove(item);
        SaveCustomQuickAccessItems();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Global.MainToastManager.CreateToast()
                .WithTitle(GetTranslation("Filemgr_QuickAccessRemoved"))
                .WithContent(item.Name)
                .OfType(NotificationType.Success)
                .Dismiss().ByClicking()
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
        });
    }

    /// <summary>
    /// <para>验证文件名是否合法（非空且不包含非法字符）。</para>
    /// Validates whether a file name is legal (non-empty and contains no invalid characters).
    /// </summary>
    /// <param name="name">
    /// <para>要验证的文件名。</para>
    /// The file name to validate.
    /// </param>
    /// <returns>
    /// <para>如果文件名合法返回 true，否则返回 false。</para>
    /// True if the file name is valid; otherwise, false.
    /// </returns>
    private static bool IsValidFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        char[] invalidChars = { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };
        return name.IndexOfAny(invalidChars) < 0;
    }

    /// <summary>
    /// <para>获取指定路径的父目录路径。</para>
    /// Gets the parent directory path of the specified path.
    /// </summary>
    /// <param name="path">
    /// <para>文件或目录的完整路径。</para>
    /// The full path of a file or directory.
    /// </param>
    /// <returns>
    /// <para>父目录路径，如果已是根目录则返回 "/"。</para>
    /// The parent directory path; returns "/" if already at root.
    /// </returns>
    private static string GetParentPath(string path)
    {
        if (string.IsNullOrEmpty(path) || path == "/")
            return "/";
        int lastSlash = path.LastIndexOf('/');
        return lastSlash <= 0 ? "/" : path[..lastSlash];
    }

    /// <summary>
    /// <para>显示未连接设备的错误对话框。</para>
    /// Shows the not-connected error dialog.
    /// </summary>
    private async Task ShowNotConnectedDialogAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Global.MainDialogManager.CreateDialog()
                .OfType(NotificationType.Error)
                .WithTitle(GetTranslation("Common_Error"))
                .WithContent(GetTranslation("Common_NotConnected"))
                .Dismiss().ByClickingBackground()
                .TryShow();
        });
    }

    /// <summary>
    /// <para>显示输入对话框并返回用户输入的字符串。若用户取消则返回 null。</para>
    /// Shows an input dialog and returns the user-entered string. Returns null if the user cancels.
    /// </summary>
    /// <param name="title">
    /// <para>对话框标题。</para>
    /// The dialog title.
    /// </param>
    /// <param name="prompt">
    /// <para>输入提示文本。</para>
    /// The input prompt text.
    /// </param>
    /// <param name="defaultValue">
    /// <para>输入框的默认值。</para>
    /// The default value for the input box.
    /// </param>
    /// <returns>
    /// <para>用户输入的字符串，取消时返回 null。</para>
    /// The user-entered string, or null if cancelled.
    /// </returns>
    private static async Task<string?> ShowInputDialogAsync(string title, string prompt, string defaultValue)
    {
        var tcs = new TaskCompletionSource<string?>();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var textBox = new TextBox { Text = defaultValue, Width = 300 };
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = prompt });
            panel.Children.Add(textBox);

            Global.MainDialogManager.CreateDialog()
                .WithTitle(title)
                .WithContent(panel)
                .WithActionButton(GetTranslation("Filemgr_Confirm"), _ => tcs.TrySetResult(textBox.Text), true)
                .WithActionButton(GetTranslation("Filemgr_Cancel"), _ => tcs.TrySetResult(null), true)
                .TryShow();
        });

        return await tcs.Task;
    }
}

/// <summary>
/// <para>快捷访问目录项，包含名称、路径、图标和激活状态。</para>
/// Quick access directory item containing name, path, icon, and active state.
/// </summary>
public partial class QuickAccessItem : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    /// <summary>
    /// <para>快捷访问项的显示名称。</para>
    /// The display name of the quick access item.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// <para>快捷访问项对应的设备目录路径。</para>
    /// The device directory path of the quick access item.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// <para>快捷访问项的图标。</para>
    /// The icon of the quick access item.
    /// </summary>
    public MaterialIconKind Icon { get; init; }

    /// <summary>
    /// <para>指示当前路径是否与此快捷访问项匹配。</para>
    /// Indicates whether the current path matches this quick access item.
    /// </summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _isActive;

    /// <summary>
    /// <para>指示此项是否为用户自定义添加的快捷访问项。自定义项可由用户移除。</para>
    /// Indicates whether this item is a user-added custom quick access item. Custom items can be removed by the user.
    /// </summary>
    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _isCustom;
}
