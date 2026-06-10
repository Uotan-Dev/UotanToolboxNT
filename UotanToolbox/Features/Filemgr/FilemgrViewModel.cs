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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Filemgr;

/// <summary>
/// <para>文件管理页面的视图模型，提供设备文件浏览与导航功能。</para>
/// View model for the file management page, providing device file browsing and navigation capabilities.
/// </summary>
public partial class FilemgrViewModel : MainPageBase
{
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
    /// <para>获取是否可以向上导航到父目录。</para>
    /// Gets whether navigation to the parent directory is possible.
    /// </summary>
    public bool CanNavigateUp => !string.IsNullOrEmpty(CurrentPath) && CurrentPath != "/";

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
        QuickAccessItems =
        [
            new QuickAccessItem { Name = "根目录", Path = "/", Icon = MaterialIconKind.HomeOutline },
            new QuickAccessItem { Name = "内部存储", Path = "/sdcard", Icon = MaterialIconKind.Harddisk },
            new QuickAccessItem { Name = "下载", Path = "/sdcard/Download", Icon = MaterialIconKind.DownloadOutline },
            new QuickAccessItem { Name = "文档", Path = "/sdcard/Documents", Icon = MaterialIconKind.FileDocumentOutline },
            new QuickAccessItem { Name = "图片", Path = "/sdcard/Pictures", Icon = MaterialIconKind.ImageOutline },
        ];
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
        if (!await GetDevicesInfo.SetDevicesInfoLittle())
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
            return;
        }

        IsBusy = true;

        // Clear existing items on UI thread first to avoid virtualization recycling conflicts
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Files.Clear();
        });

        try
        {
            // Ensure trailing slash so symlinks like /sdcard are followed into the directory
            string listPath = path.EndsWith("/") ? path : path + "/";
            string output = await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell ls -la \"{listPath}\"");

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
    /// <para>将设备上的文件或目录拉取到本地。弹出文件夹选择对话框让用户选择保存位置，然后执行 ADB pull 命令。</para>
    /// Pulls a file or directory from the device to local. Shows a folder picker dialog for the save location, then executes the ADB pull command.
    /// </summary>
    /// <param name="entry">
    /// <para>要拉取的文件条目。</para>
    /// The file entry to pull.
    /// </param>
    [RelayCommand]
    private async Task PullFileAsync(FileEntry entry)
    {
        if (entry is null)
            return;

        if (!await GetDevicesInfo.SetDevicesInfoLittle())
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
            return;
        }

        try
        {
            var mainWindow = (Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow == null)
                return;

            System.Collections.Generic.IReadOnlyList<IStorageFolder> folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Open Folder",
                AllowMultiple = false
            });
            if (folders.Count < 1)
                return;
            string localPath = folders[0].TryGetLocalPath() ?? string.Empty;
            if (string.IsNullOrEmpty(localPath))
                return;

            string output = await FeaturesHelper.AdbCmd(Global.thisdevice, $"pull \"{entry.FullPath}\" \"{localPath}\"");

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (output.Contains("bytes in", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("file pulled", StringComparison.OrdinalIgnoreCase) ||
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
        catch (Exception ex)
        {
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

        if (!await GetDevicesInfo.SetDevicesInfoLittle())
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
            return;
        }

        try
        {
            var tcs = new TaskCompletionSource<string>();
            var textBox = new TextBox { Text = entry.Name, Width = 300 };
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = GetTranslation("Filemgr_EnterNewName") });
            panel.Children.Add(textBox);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Filemgr_RenameTitle"))
                    .WithContent(panel)
                    .WithActionButton(GetTranslation("Filemgr_Confirm"), _ => tcs.TrySetResult(textBox.Text ?? string.Empty), true)
                    .WithActionButton(GetTranslation("Filemgr_Cancel"), _ => tcs.TrySetResult(null), true)
                    .TryShow();
            });

            string newName = await tcs.Task;
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
            string output = await FeaturesHelper.AdbCmd(Global.thisdevice, $"shell mv \"{entry.FullPath}\" \"{newFullPath}\"");

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

        if (!await GetDevicesInfo.SetDevicesInfoLittle())
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
            return;
        }

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
                ? $"shell rm -rf \"{entry.FullPath}\""
                : $"shell rm \"{entry.FullPath}\"";
            string output = await FeaturesHelper.AdbCmd(Global.thisdevice, cmd);

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
    public string Name { get; init; }

    /// <summary>
    /// <para>快捷访问项对应的设备目录路径。</para>
    /// The device directory path of the quick access item.
    /// </summary>
    public string Path { get; init; }

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
}
