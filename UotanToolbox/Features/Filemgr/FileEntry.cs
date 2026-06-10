using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using System.IO;

namespace UotanToolbox.Features.Filemgr;

/// <summary>
/// <para>文件条目数据模型，表示设备上的文件或目录信息。</para>
/// Data model for a file entry, representing file or directory information on a device.
/// </summary>
public partial class FileEntry : ObservableObject
{
    /// <summary>
    /// <para>文件或目录名称。</para>
    /// The name of the file or directory.
    /// </summary>
    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>
    /// <para>设备上的绝对路径。</para>
    /// The absolute path on the device.
    /// </summary>
    [ObservableProperty]
    private string _fullPath = string.Empty;

    /// <summary>
    /// <para>文件大小（字节）。</para>
    /// The size of the file in bytes.
    /// </summary>
    [ObservableProperty]
    private long _size;

    /// <summary>
    /// <para>修改时间字符串。</para>
    /// The modification time string.
    /// </summary>
    [ObservableProperty]
    private string _modifiedTime = string.Empty;

    /// <summary>
    /// <para>是否为目录。</para>
    /// Whether this entry is a directory.
    /// </summary>
    [ObservableProperty]
    private bool _isDirectory;

    /// <summary>
    /// <para>是否为符号链接。</para>
    /// Whether this entry is a symbolic link.
    /// </summary>
    [ObservableProperty]
    private bool _isSymlink;

    /// <summary>
    /// <para>符号链接的目标路径，非符号链接时为空字符串。</para>
    /// The target path of the symbolic link; empty string if not a symlink.
    /// </summary>
    [ObservableProperty]
    private string _symlinkTarget = string.Empty;

    /// <summary>
    /// <para>权限字符串，如 "drwxrwx---"。</para>
    /// The permission string, e.g., "drwxrwx---".
    /// </summary>
    [ObservableProperty]
    private string _permission = string.Empty;

    /// <summary>
    /// <para>所有者名称。</para>
    /// The owner name.
    /// </summary>
    [ObservableProperty]
    private string _owner = string.Empty;

    /// <summary>
    /// <para>所属组名称。</para>
    /// The group name.
    /// </summary>
    [ObservableProperty]
    private string _group = string.Empty;

    /// <summary>
    /// <para>UI显示名称，默认与Name相同，可自定义。</para>
    /// The display name for UI; defaults to Name but can be customized.
    /// </summary>
    [ObservableProperty]
    private string _displayName = string.Empty;

    /// <summary>
    /// <para>人类可读的大小字符串，如 "1.5 KB"、"2.3 MB"。</para>
    /// Human-readable size string, e.g., "1.5 KB", "2.3 MB".
    /// </summary>
    [ObservableProperty]
    private string _displaySize = "0 B";

    /// <summary>
    /// <para>根据文件类型返回对应的 Material 图标种类。</para>
    /// Returns the corresponding Material icon kind based on the file type.
    /// </summary>
    public MaterialIconKind IconKind
    {
        get
        {
            if (IsDirectory)
                return MaterialIconKind.Folder;

            if (IsSymlink)
                return MaterialIconKind.LinkVariant;

            string ext = Path.GetExtension(Name).ToLowerInvariant();
            return ext switch
            {
                ".apk" => MaterialIconKind.Android,
                ".zip" or ".rar" or ".7z" or ".tar" or ".gz" or ".bz2" => MaterialIconKind.ZipBoxOutline,
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg" => MaterialIconKind.ImageOutline,
                ".mp3" or ".wav" or ".flac" or ".ogg" or ".aac" or ".m4a" => MaterialIconKind.MusicNote,
                ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" or ".flv" or ".webm" => MaterialIconKind.VideoOutline,
                ".txt" or ".log" or ".md" or ".csv" or ".xml" or ".json" or ".ini" or ".cfg" or ".conf" => MaterialIconKind.FileDocumentOutline,
                ".pdf" => MaterialIconKind.FilePdfBox,
                ".sh" or ".bat" => MaterialIconKind.Console,
                _ => MaterialIconKind.FileOutline
            };
        }
    }

    /// <summary>
    /// <para>将字节数转换为人类可读的大小字符串。</para>
    /// Converts a byte count to a human-readable size string.
    /// </summary>
    /// <param name="size">
    /// <para>文件大小（字节）。</para>
    /// The file size in bytes.
    /// </param>
    /// <returns>
    /// <para>人类可读的大小字符串，如 "512 B"、"1.5 KB"、"2.3 MB"、"4.0 GB"。</para>
    /// A human-readable size string, e.g., "512 B", "1.5 KB", "2.3 MB", "4.0 GB".
    /// </returns>
    public static string GetDisplaySize(long size)
    {
        if (size < 0)
            size = 0;

        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double len = size;
        int order = 0;

        while (len >= 1024 && order < units.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return order == 0 ? $"{len} {units[order]}" : $"{len:F1} {units[order]}";
    }

    partial void OnSizeChanged(long value)
    {
        DisplaySize = GetDisplaySize(value);
    }

    partial void OnIsDirectoryChanged(bool value)
    {
        OnPropertyChanged(nameof(IconKind));
    }

    partial void OnNameChanged(string value)
    {
        OnPropertyChanged(nameof(IconKind));
    }
}
