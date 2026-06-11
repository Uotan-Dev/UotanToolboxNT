using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using FirmwareKit.Lp;
using FirmwareKit.Nb0;
using FirmwareKit.NTPi;
using FirmwareKit.Oppo;
using FirmwareKit.Oppo.Crypto;
using FirmwareKit.Oppo.Models;
using FirmwareKit.OzipReader;
using FirmwareKit.OpsReader;
using FirmwareKit.OfpReader;
using FirmwareKit.Sparse.Core;
using FirmwareKit.Sparse.Models;
using FirmwareKit.Sparse.Streams;
using ReactiveUI;
using SukiUI.Dialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Common.Devices;
using UotanToolbox.Common.PatchHelper;
using UotanToolbox.Common.ROMHelper;
using UotanToolbox.Features.Wiredflash;
using UotanToolbox.Utilities;
using ZstdSharp;

namespace UotanToolbox.Features.Advancedflash;

public partial class AdvancedflashView : UserControl
{
    private enum ParsedFileType
    {
        Unknown,
        Script,
        Payload,
        Super,
        PayloadUrl,
        Ntpi,
        Nb0,
        Ozip,
        Ops,
        Ofp
    }

    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public AvaloniaList<string> ScriptList = [".bat", ".sh", ".txt"];
    private LpMetadata? _metadata;
    private ParsedFileType _parsedFileType = ParsedFileType.Unknown;
    private readonly string fastboot_log_path = Path.Combine(Global.log_path, "fastboot.txt");
    private string output = "";
    private readonly StringBuilder _logBuffer = new();
    private readonly object _logBufferLock = new();
    private OppCryptoProvider? _oppCryptoProvider;
    private readonly DispatcherTimer _logFlushTimer;

    public AdvancedflashView()
    {
        InitializeComponent();
        Total.Text = "0";
        //ExportScr.ItemsSource = ScriptList;
        _logFlushTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _logFlushTimer.Tick += (_, _) => FlushLogBuffer();
        _ = this.WhenAnyValue(part => part.SearchBox.Text)
            .Subscribe(option =>
            {
                if (ImgList.ItemsSource != null)
                {
                    var vm = GetViewModel();
                    AvaloniaList<FalshPartModel> Parts = vm.FalshPartModel;
                    if (!string.IsNullOrEmpty(SearchBox.Text))
                    {
                        ImgList.ItemsSource = Parts.Where(part => part.Name.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    else
                    {
                        ImgList.ItemsSource = Parts.Where(info => info != null).ToList();
                    }
                }
            });
    }

    private async void SetEnabled(bool Set)
    {
        if (Set)
        {
            BusyFlash.IsBusy = true;
            ImgList.IsEnabled = false;
            Read.IsEnabled = false;
            FlashSelectBut.IsEnabled = false;
            Erase.IsEnabled = false;
            SetOt.IsEnabled = false;
            ExtractSelect.IsEnabled = false;
            ToWiredPkgBut.IsEnabled = false;
        }
        else
        {
            BusyFlash.IsBusy = false;
            ImgList.IsEnabled = true;
            Read.IsEnabled = true;
            FlashSelectBut.IsEnabled = true;
            Erase.IsEnabled = true;
            SetOt.IsEnabled = true;
            ExtractSelect.IsEnabled = true;
            ToWiredPkgBut.IsEnabled = true;
        }
    }

    private Task AppendFastbootOutputAsync(string commandOutput)
    {
        if (commandOutput == null)
        {
            return Task.CompletedTask;
        }

        string normalizedOutput = commandOutput
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Replace("\0", string.Empty);

        lock (_logBufferLock)
        {
            _logBuffer.Append(normalizedOutput);
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!_logFlushTimer.IsEnabled)
                _logFlushTimer.Start();
        });

        return Task.CompletedTask;
    }

    private void FlushLogBuffer()
    {
        string text;
        lock (_logBufferLock)
        {
            if (_logBuffer.Length == 0)
            {
                _logFlushTimer.Stop();
                return;
            }
            text = _logBuffer.ToString();
            _logBuffer.Clear();
        }

        AdvancedflashLog.Text += text;
        AdvancedflashLog.CaretIndex = AdvancedflashLog.Text.Length;
        output += text;
    }

    public async Task Fastboot(string fbshell)
    {
        if (Global.DeviceManager != null)
        {
            var dev = Global.DeviceManager.Devices.FirstOrDefault(d => d.Id == Global.thisdevice && d.Transport == TransportType.Fastboot);
            if (dev != null)
            {
                _ = await Global.DeviceManager.ExecuteStreamingAsync(dev, fbshell, chunk => _ = AppendFastbootOutputAsync(chunk));
                return;
            }
        }

        _ = await CallExternalProgram.Fastboot(fbshell, chunk => _ = AppendFastbootOutputAsync(chunk));
    }

    private AdvancedflashViewModel GetViewModel()
    {
        return DataContext as AdvancedflashViewModel
            ?? throw new InvalidOperationException("DataContext is not AdvancedflashViewModel.");
    }

    private static FilePickerFileType FlashPicker { get; } = new("File")
    {
        Patterns = new[] { "*.img", "*.bin", "*.zip", "*.txt", "*.bat", ".sh", "*.ntpi", "*.nb0", "*.ozip", "*.ops", "*.ofp" },
        AppleUniformTypeIdentifiers = new[] { "*.img", "*.bin", "*.zip", "*.txt", "*.bat", ".sh", "*.ntpi", "*.nb0", "*.ozip", "*.ops", "*.ofp" }
    };

    private async void OpenFile(object sender, RoutedEventArgs args)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
        {
            return;
        }

        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false,
            FileTypeFilter = new[] { FlashPicker }
        });
        if (files.Count >= 1)
        {
            var localPath = files[0].TryGetLocalPath();
            if (string.IsNullOrWhiteSpace(localPath))
            {
                AdvancedflashLog.Text += "\nInvalid selected file path.";
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_SelectTip")).Dismiss().ByClickingBackground().TryShow();
                return;
            }

            string path = File.Text = localPath;
            BusyFlash.IsBusy = true;

            var extension = Path.GetExtension(path);
            if (ScriptList.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                _parsedFileType = ParsedFileType.Script;
                AdvancedflashLog.Text += $"\nSkip script type: {Path.GetFileName(path)}";
                //检测后的脚本逻辑写这里
                string fileExtension = Path.GetExtension(path);
                switch (fileExtension)
                {
                    case ".bat":
                    case ".sh":
                        try
                        {
                            var vm = GetViewModel();
                            vm.FalshPartModel.Clear();
                            await Task.Delay(100);

                            string[] lines = await System.IO.File.ReadAllLinesAsync(path);
                            string scriptDir = Path.GetDirectoryName(path) ?? string.Empty;

                            // 匹配 fastboot [可选的%*或$*] 命令 [可选的分区名/参数] [可选的路径]
                            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"fastboot\s+(?:(?:%\*|\$\*)\s+)?(flash|erase|set_active|reboot)(?:\s+([^\s]+))?(?:\s+(.*?))?(?=\s*(?:\||&|;|>|$))", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                            foreach (string line in lines)
                            {
                                var match = regex.Match(line);
                                if (match.Success)
                                {
                                    string command = match.Groups[1].Value.ToLower(); // flash, erase, set_active, reboot
                                    string partitionOrArg = match.Groups[2].Value;    // partition name e.g. boot_ab, or 'a' for set_active
                                    string relativeFilePath = match.Groups[3].Value;  // file path e.g. %~dp0images/boot.img

                                    string fullPath = "";
                                    string fileName = "";
                                    string sizeStr = "";
                                    string partName = partitionOrArg;

                                    if (command == "set_active")
                                    {
                                        partName = partitionOrArg; // 参数 'a' 或 'b' 实际在第2个捕获组
                                    }
                                    else if (command == "reboot")
                                    {
                                        partName = "";
                                    }

                                    if (!string.IsNullOrEmpty(relativeFilePath) && command != "set_active" && command != "reboot")
                                    {
                                        // 移除批处理和sh脚本中的特殊变量以及引号
                                        string normalizedPath = relativeFilePath
                                            .Replace("%~dp0", "")
                                            .Replace("`dirname $0`", "")
                                            .Replace("$(dirname $0)", "")
                                            .Replace("\"", "")
                                            .Trim();

                                        // 处理.sh等脚本中的常规路径表示 (例如当前目录 ./)
                                        if (normalizedPath.StartsWith("./") || normalizedPath.StartsWith(".\\"))
                                        {
                                            normalizedPath = normalizedPath.Substring(2);
                                        }

                                        // 移除开头的斜杠，防止 Path.Combine 识别为绝对路径导致拼接错误
                                        normalizedPath = normalizedPath.TrimStart('/', '\\');

                                        // 标准化路径分隔符并拼接完整路径
                                        normalizedPath = normalizedPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                                        fullPath = Path.Combine(scriptDir, normalizedPath);
                                        fileName = Path.GetFileName(fullPath);

                                        if (partName.Replace(" ", "") == "super" && !System.IO.File.Exists(fullPath))
                                        {
                                            // Change the extension to .zst and check again
                                            string zstPath = Path.ChangeExtension(fullPath, ".zst");
                                            if (System.IO.File.Exists(zstPath))
                                            {
                                                fullPath = zstPath;
                                                fileName = Path.GetFileName(zstPath);
                                            }
                                        }

                                        // 如果文件存在，计算大小
                                        if (System.IO.File.Exists(fullPath))
                                        {
                                            sizeStr = StringHelper.byte2AUnit((ulong)new System.IO.FileInfo(fullPath).Length);
                                        }
                                    }

                                    vm.FalshPartModel.Add(new FalshPartModel
                                    {
                                        Select = false,
                                        Name = partName.Replace(" ", ""),
                                        Size = sizeStr,
                                        Command = command,
                                        FileName = fileName,
                                        FullFilePath = fullPath
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            AdvancedflashLog.Text += $"Error: {ex.Message}";
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
                        }
                        break;
                    case ".txt":
                        try
                        {
                            var vm = GetViewModel();
                            vm.FalshPartModel.Clear();
                            await Task.Delay(100);

                            string[] lines = await System.IO.File.ReadAllLinesAsync(path);
                            string scriptDir = Path.GetDirectoryName(path) ?? string.Empty;

                            foreach (string line in lines)
                            {
                                string trimmedLine = line.Trim();
                                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;
                                if (trimmedLine.StartsWith("codename:", StringComparison.OrdinalIgnoreCase)) continue;

                                string[] parts = trimmedLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length > 0)
                                {
                                    string partName = parts[0];
                                    string command = "flash";
                                    string relativeFilePath = "";
                                    string fullPath = "";
                                    string fileName = "";
                                    string sizeStr = "";

                                    if (parts.Length == 1)
                                    {
                                        // 默认镜像路径：同级目录下的 images/分区名.img
                                        relativeFilePath = Path.Combine("images", $"{partName}.img");
                                        fileName = $"{partName}.img"; // 即使文件不存在也显示文件名
                                    }
                                    else if (parts.Length >= 2)
                                    {
                                        string arg = parts[1];
                                        if (arg.Equals("delete", StringComparison.OrdinalIgnoreCase))
                                        {
                                            command = "delete";
                                        }
                                        else if (arg.Equals("create", StringComparison.OrdinalIgnoreCase))
                                        {
                                            command = "create";
                                        }
                                        else
                                        {
                                            relativeFilePath = arg;
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(relativeFilePath))
                                    {
                                        // 移除开头的斜杠或反斜杠，防止 Path.Combine 识别为绝对路径导致拼接错误
                                        string normalizedPath = relativeFilePath.TrimStart('/', '\\');
                                        normalizedPath = normalizedPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

                                        fullPath = Path.Combine(scriptDir, normalizedPath);
                                        fileName = Path.GetFileName(fullPath);

                                        // 如果文件存在，计算大小
                                        if (System.IO.File.Exists(fullPath))
                                        {
                                            sizeStr = StringHelper.byte2AUnit((ulong)new System.IO.FileInfo(fullPath).Length);
                                        }
                                    }

                                    vm.FalshPartModel.Add(new FalshPartModel
                                    {
                                        Select = false,
                                        Name = partName.Replace(" ", ""),
                                        Size = sizeStr,
                                        Command = command.Replace(" ", ""),
                                        FileName = fileName,
                                        FullFilePath = fullPath
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            AdvancedflashLog.Text += $"Error: {ex.Message}";
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
                        }
                        break;
                }
                BusyFlash.IsBusy = false;
                return;
            }

            var fmtExtension = Path.GetExtension(path).ToLowerInvariant();
            switch (fmtExtension)
            {
                case ".ntpi":
                    _parsedFileType = ParsedFileType.Ntpi;
                    if (await TryParseNtpiAsync(path))
                    {
                        BusyFlash.IsBusy = false;
                        return;
                    }
                    break;
                case ".nb0":
                    _parsedFileType = ParsedFileType.Nb0;
                    if (await TryParseNb0Async(path))
                    {
                        BusyFlash.IsBusy = false;
                        return;
                    }
                    break;
                case ".ozip":
                    _parsedFileType = ParsedFileType.Ozip;
                    if (await TryParseOppoAsync(path))
                    {
                        BusyFlash.IsBusy = false;
                        return;
                    }
                    break;
                case ".ops":
                    _parsedFileType = ParsedFileType.Ops;
                    if (await TryParseOppoAsync(path))
                    {
                        BusyFlash.IsBusy = false;
                        return;
                    }
                    break;
                case ".ofp":
                    _parsedFileType = ParsedFileType.Ofp;
                    if (await TryParseOppoAsync(path))
                    {
                        BusyFlash.IsBusy = false;
                        return;
                    }
                    break;
            }

            try
            {
                var parsed = await TryParseAndPushToUiAsync(path);
                if (!parsed)
                {
                    AdvancedflashLog.Text += $"\nUnsupported format: {Path.GetFileName(path)}";
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_Unsupport")).Dismiss().ByClickingBackground().TryShow();
                }

                TrySyncFileNamesFromUnpackFolder(path);
            }
            catch (Exception ex)
            {
                AdvancedflashLog.Text += $"Error: {ex.Message}";
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
            }
            finally
            {
                BusyFlash.IsBusy = false;
            }
            BusyFlash.IsBusy = false;
        }
    }

    private async Task<bool> TryParseAndPushToUiAsync(string path)
    {
        try
        {
            // 优先尝试解析 payload
            if (await TryParsePayloadAsync(path))
            {
                return true;
            }

            // 如果不是 payload，再尝试解析 super 镜像
            if (Path.GetExtension(path).Equals(".img", StringComparison.OrdinalIgnoreCase) && await TryParseSuperAsync(path))
            {
                return true;
            }
        }
        catch (FileNotFoundException ex)
        {
            AdvancedflashLog.Text += $"\nError: File not found - {ex.Message}";
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_FileNotFound")).Dismiss().ByClickingBackground().TryShow();
        }
        catch (InvalidOperationException ex)
        {
            AdvancedflashLog.Text += $"\nError: Invalid operation - {ex.Message}";
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_Invalid")).Dismiss().ByClickingBackground().TryShow();
        }
        catch (Exception ex)
        {
            AdvancedflashLog.Text += $"\nError: An unexpected error occurred - {ex.Message}";
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_Unexpected")).Dismiss().ByClickingBackground().TryShow();
        }

        _parsedFileType = ParsedFileType.Unknown;
        AdvancedflashLog.Text += $"\nUnsupported format: {Path.GetFileName(path)}";
        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_Unsupport")).Dismiss().ByClickingBackground().TryShow();
        return false;
    }

    private async Task<bool> TryParsePayloadAsync(string path)
    {
        try
        {
            var parts = await PayloadParser.GetPartitionInfoAsync(path);
            if (parts == null || parts.Count == 0)
            {
                return false;
            }

            var vm = GetViewModel();
            vm.FalshPartModel.Clear();
            await Task.Delay(100);
            foreach (var p in parts)
            {
                vm.FalshPartModel.Add(new FalshPartModel
                {
                    Select = false,
                    Name = p.Name,
                    Size = p.SizeReadable,
                    Command = "",
                    FileName = ""
                });
            }
            AdvancedflashLog.Text += $"Payload.bin Detected";
            _parsedFileType = ParsedFileType.Payload;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> TryParseSuperAsync(string path)
    {
        try
        {
            _metadata = await Task.Run(() =>
            {
                using var fs = System.IO.File.OpenRead(path);
                var magicBuf = new byte[4];
                var isSparse = false;
                if (fs.Read(magicBuf, 0, 4) == 4)
                {
                    if (BitConverter.ToUInt32(magicBuf, 0) == SparseFormat.SparseHeaderMagic)
                    {
                        isSparse = true;
                    }
                }

                if (isSparse)
                {
                    using var sparseFile = SparseFile.FromImageFile(path);
                    using var inputStream = new SparseStream(sparseFile);
                    var metadataReader = new MetadataReader();
                    return metadataReader.ReadFromImageStream(inputStream);
                }

                using var rawInputStream = System.IO.File.OpenRead(path);
                var rawMetadataReader = new MetadataReader();
                return rawMetadataReader.ReadFromImageStream(rawInputStream);
            });

            if (_metadata == null || _metadata.Partitions.Count == 0)
            {
                return false;
            }

            var vm = GetViewModel();
            vm.FalshPartModel.Clear();
            await Task.Delay(100);
            foreach (var p in _metadata.Partitions)
            {
                ulong totalSize = 0;
                for (uint i = 0; i < p.NumExtents; i++)
                {
                    totalSize += _metadata.Extents[(int)(p.FirstExtentIndex + i)].NumSectors * 512;
                }

                vm.FalshPartModel.Add(new FalshPartModel
                {
                    Select = false,
                    Name = p.GetName(),
                    Size = StringHelper.byte2AUnit(totalSize),
                    Command = "",
                    FileName = ""
                });
            }

            _parsedFileType = ParsedFileType.Super;
            AdvancedflashLog.Text += $"Super Detected";
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// <para>尝试将文件解析为 NTPI 格式并推送分区列表到 UI。</para>
    /// Attempts to parse the file as NTPI format and push partition list to UI.
    /// </summary>
    private async Task<bool> TryParseNtpiAsync(string path)
    {
        try
        {
            var fileInfo = await Task.Run(() =>
            {
                var reader = new NtpiReader(new FirmwareKit.NTPi.Crypto.AesCbcCryptoProvider(), new FirmwareKit.NTPi.Compression.Lzma2Compressor());
                return reader.ReadInfo(path);
            });

            if (fileInfo == null || fileInfo.FileEntries == null || fileInfo.FileEntries.Count == 0)
            {
                return false;
            }

            var vm = GetViewModel();
            vm.FalshPartModel.Clear();
            await Task.Delay(100);
            foreach (var entry in fileInfo.FileEntries)
            {
                vm.FalshPartModel.Add(new FalshPartModel
                {
                    Select = false,
                    Name = entry.Name ?? "",
                    Size = entry.OriginalLength > 0 ? StringHelper.byte2AUnit((ulong)entry.OriginalLength) : "",
                    Command = "",
                    FileName = ""
                });
            }
            AdvancedflashLog.Text += $"\nNTPI Detected - {fileInfo.FileEntries.Count} entries";
            _parsedFileType = ParsedFileType.Ntpi;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// <para>尝试将文件解析为 NB0 格式并推送镜像列表到 UI。</para>
    /// Attempts to parse the file as NB0 format and push image list to UI.
    /// </summary>
    private async Task<bool> TryParseNb0Async(string path)
    {
        try
        {
            var nb0Info = await Task.Run(() =>
            {
                return Nb0Parser.Parse(path);
            });

            if (nb0Info == null || nb0Info.Entries == null || nb0Info.EntryCount == 0)
            {
                return false;
            }

            var vm = GetViewModel();
            vm.FalshPartModel.Clear();
            await Task.Delay(100);
            foreach (var entry in nb0Info.Entries)
            {
                vm.FalshPartModel.Add(new FalshPartModel
                {
                    Select = false,
                    Name = entry.Name ?? "",
                    Size = entry.Size > 0 ? StringHelper.byte2AUnit((ulong)entry.Size) : "",
                    Command = "",
                    FileName = ""
                });
            }
            AdvancedflashLog.Text += $"\nNB0 Detected - {nb0Info.EntryCount} entries";
            _parsedFileType = ParsedFileType.Nb0;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// <para>获取或创建 Oppo 加密提供者实例，并注册所有格式特定的加密提供者。</para>
    /// Gets or creates an Oppo crypto provider instance with all format-specific providers registered.
    /// </summary>
    private OppCryptoProvider GetOrCreateOppCryptoProvider()
    {
        if (_oppCryptoProvider != null)
            return _oppCryptoProvider;

        _oppCryptoProvider = new OppCryptoProvider(null);
        OzipPackageInitializer.Initialize(_oppCryptoProvider);
        OpsPackageInitializer.Register(_oppCryptoProvider);
        OfpPackageInitializer.Initialize(_oppCryptoProvider, null);
        return _oppCryptoProvider;
    }

    /// <summary>
    /// <para>尝试将文件解析为 Oppo 固件格式（OZIP/OPS/OFP）并推送分区列表到 UI。</para>
    /// Attempts to parse the file as an Oppo firmware format (OZIP/OPS/OFP) and push partition list to UI.
    /// </summary>
    private async Task<bool> TryParseOppoAsync(string path)
    {
        try
        {
            var cryptoProvider = GetOrCreateOppCryptoProvider();
            var archive = await Task.Run(() =>
            {
                var reader = new OppReader(cryptoProvider);
                return reader.Parse(path);
            });

            if (archive == null || archive.Entries == null || archive.Entries.Count == 0)
                return false;

            var vm = GetViewModel();
            vm.FalshPartModel.Clear();
            await Task.Delay(100);
            foreach (var entry in archive.Entries)
            {
                vm.FalshPartModel.Add(new FalshPartModel
                {
                    Select = false,
                    Name = entry.Name ?? "",
                    Size = entry.Size > 0 ? StringHelper.byte2AUnit((ulong)entry.Size) : "",
                    Command = "",
                    FileName = ""
                });
            }

            var formatName = archive.Metadata?.FormatName ?? "OPPO";
            AdvancedflashLog.Text += $"\n{formatName} Detected - {archive.Entries.Count} entries";

            if (archive.Metadata?.Format == OppFormat.Ozip)
                _parsedFileType = ParsedFileType.Ozip;
            else if (archive.Metadata?.Format == OppFormat.Ops)
                _parsedFileType = ParsedFileType.Ops;
            else if (archive.Metadata?.Format == OppFormat.OfpQc || archive.Metadata?.Format == OppFormat.OfpMtk)
                _parsedFileType = ParsedFileType.Ofp;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// <para>从 Oppo 固件文件（OZIP/OPS/OFP）中提取选中的分区镜像。</para>
    /// Extracts selected partition images from an Oppo firmware file (OZIP/OPS/OFP).
    /// </summary>
    private async Task ExtractOppoSelectedAsync(string sourcePath, string outputDir, List<FalshPartModel> selectedParts)
    {
        var selectedNames = selectedParts
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var cryptoProvider = GetOrCreateOppCryptoProvider();
        await Task.Run(() =>
        {
            var reader = new OppReader(cryptoProvider);
            reader.Extract(sourcePath, outputDir);
        });

        // Match extracted files to partition names and remove unselected files
        if (Directory.Exists(outputDir))
        {
            foreach (var file in Directory.GetFiles(outputDir, "*", SearchOption.TopDirectoryOnly))
            {
                var fileName = Path.GetFileName(file);
                var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);

                var matchedPart = selectedParts.FirstOrDefault(p =>
                    string.Equals(p.Name, nameWithoutExt, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.Name, fileName, StringComparison.OrdinalIgnoreCase));

                if (matchedPart != null)
                {
                    matchedPart.FileName = fileName;
                    matchedPart.FullFilePath = file;
                }
                else if (selectedNames.Count > 0)
                {
                    try { System.IO.File.Delete(file); } catch { }
                }
            }
        }
    }

    private async void OpenFolder(object sender, RoutedEventArgs args)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
        {
            return;
        }

        System.Collections.Generic.IReadOnlyList<IStorageFolder> files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Open Folder",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            string? folderPath = files[0].TryGetLocalPath();
            File.Text = folderPath;
            BusyFlash.IsBusy = true;
            try
            {
                var vm = GetViewModel();
                vm.FalshPartModel.Clear();
                await Task.Delay(100);

                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    var imgFiles = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);

                    foreach (var file in imgFiles)
                    {
                        var fileInfo = new FileInfo(file);
                        string fileName = fileInfo.Name;
                        string partName = Path.GetFileNameWithoutExtension(fileName).Replace(" ", "");
                        string sizeStr = StringHelper.byte2AUnit((ulong)fileInfo.Length);

                        vm.FalshPartModel.Add(new FalshPartModel
                        {
                            Select = false,
                            Name = partName,
                            Size = sizeStr,
                            Command = "flash",
                            FileName = fileName,
                            FullFilePath = file
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                AdvancedflashLog.Text += $"Error: {ex.Message}";
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
            }
            finally
            {
                BusyFlash.IsBusy = false;
            }
        }
    }

    private async void OpenUrl(object sender, RoutedEventArgs args)
    {
        BusyFlash.IsBusy = true;
        try
        {
            if (Uri.IsWellFormedUriString(File.Text, UriKind.Absolute))
            {
                var parts = await PayloadParser.GetPartitionInfoFromUrlAsync(File.Text);
                var vm = GetViewModel();
                vm.FalshPartModel.Clear();
                foreach (var p in parts)
                {
                    if (p.SizeBytes > 314572800)
                    {
                        vm.FalshPartModel.Add(new FalshPartModel
                        {
                            Select = false,
                            SelectDis = false,
                            Name = p.Name,
                            Size = p.SizeReadable,
                            Command = "请下载完整包",
                            FileName = ""
                        });
                    }
                    else
                    {
                        vm.FalshPartModel.Add(new FalshPartModel
                        {
                            Select = false,
                            Name = p.Name,
                            Size = p.SizeReadable,
                            Command = "",
                            FileName = ""
                        });
                    }
                }

                _parsedFileType = ParsedFileType.PayloadUrl;
                TrySyncFileNamesFromUnpackFolder(File.Text, true);
            }
            else
            {
                _parsedFileType = ParsedFileType.Unknown;
                UrlUtilities.OpenURL("https://xiaomirom.com/series/");
            }
        }
        catch (Exception ex)
        {
            AdvancedflashLog.Text += $"Error: {ex.Message}";
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
        }
        finally
        {
            BusyFlash.IsBusy = false;
        }
        BusyFlash.IsBusy = false;
    }

    private async void Extract(object sender, RoutedEventArgs args)
    {
        SetEnabled(true);
        try
        {
            var sourcePath = File.Text ?? string.Empty;
            var isUrl = Uri.IsWellFormedUriString(sourcePath, UriKind.Absolute);
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                AdvancedflashLog.Text += "\nInvalid image path.";
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_SelectTip")).Dismiss().ByClickingBackground().TryShow();
                return;
            }

            if (!isUrl && !System.IO.File.Exists(sourcePath))
            {
                AdvancedflashLog.Text += "\nInvalid image path.";
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_SelectTip")).Dismiss().ByClickingBackground().TryShow();
                return;
            }

            if (isUrl && _parsedFileType != ParsedFileType.PayloadUrl)
            {
                AdvancedflashLog.Text += "\nCurrent URL is not in payload_url mode.";
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_UrlNotPayload")).Dismiss().ByClickingBackground().TryShow();
                return;
            }

            var vm = GetViewModel();
            var selectedParts = vm.FalshPartModel.Where(x => x.Select).ToList();
            if (selectedParts.Count == 0)
            {
                AdvancedflashLog.Text += "\nNo partition selected.";
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_NoPart")).Dismiss().ByClickingBackground().TryShow();
                return;
            }

            var outputDir = GetUnpackOutputDir(sourcePath, isUrl);
            Directory.CreateDirectory(outputDir);

            switch (_parsedFileType)
            {
                case ParsedFileType.Payload:
                    await ExtractPayloadSelectedAsync(sourcePath, outputDir, selectedParts);
                    break;
                case ParsedFileType.Super:
                    await ExtractSuperSelectedAsync(sourcePath, outputDir, selectedParts);
                    break;
                case ParsedFileType.PayloadUrl:
                    await ExtractPayloadUrlSelectedAsync(sourcePath, outputDir, selectedParts.Select(x => x.Name).ToArray());
                    break;
                case ParsedFileType.Ntpi:
                    await ExtractNtpiSelectedAsync(sourcePath, outputDir, selectedParts);
                    break;
                case ParsedFileType.Nb0:
                    await ExtractNb0SelectedAsync(sourcePath, outputDir, selectedParts);
                    break;
                case ParsedFileType.Ozip:
                case ParsedFileType.Ops:
                case ParsedFileType.Ofp:
                    await ExtractOppoSelectedAsync(sourcePath, outputDir, selectedParts);
                    break;
                default:
                    AdvancedflashLog.Text += "\nUnknown image type. Please re-open image file first.";
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_Unsupport")).Dismiss().ByClickingBackground().TryShow();
                    return;
            }

            var successCount = 0;
            foreach (var item in selectedParts)
            {
                var outPath = Path.Combine(outputDir, $"{item.Name}.img");
                if (System.IO.File.Exists(outPath))
                {
                    item.FileName = Path.GetFileName(outPath);
                    item.FullFilePath = outPath;
                    successCount++;
                }
            }
            FileHelper.OpenFolder(Path.Combine(outputDir));
            AdvancedflashLog.Text += $"\nExtract finished: {successCount}/{selectedParts.Count} -> {outputDir}";
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
        }
        catch (Exception ex)
        {
            AdvancedflashLog.Text += $"Error: {ex.Message}";
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
        }
        finally
        {
            SetEnabled(false);
        }
    }

    private static async Task ExtractPayloadSelectedAsync(string sourcePath, string outputDir, List<FalshPartModel> selectedParts)
    {
        var names = selectedParts
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var previousDirectory = Directory.GetCurrentDirectory();
        try
        {
            // PayloadHelper writes files to current directory, so switch temporarily.
            Directory.SetCurrentDirectory(outputDir);
            await PayloadParser.ExtractSelectedPartitionsAsync(sourcePath, names);
        }
        finally
        {
            Directory.SetCurrentDirectory(previousDirectory);
        }
    }

    private static async Task ExtractSuperSelectedAsync(string sourcePath, string outputDir, List<FalshPartModel> selectedParts)
    {
        var names = selectedParts
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var selectedNameSet = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);

        await Task.Run(() =>
        {
            using var fs = System.IO.File.OpenRead(sourcePath);
            var magicBuf = new byte[4];
            var isSparse = false;
            if (fs.Read(magicBuf, 0, 4) == 4)
            {
                isSparse = BitConverter.ToUInt32(magicBuf, 0) == SparseFormat.SparseHeaderMagic;
            }

            if (isSparse)
            {
                using var sparseFile = SparseFile.FromImageFile(sourcePath);
                using var sparseStream = new SparseStream(sparseFile);
                ExtractSelectedSuperPartitions(sparseStream, outputDir, selectedNameSet);
            }
            else
            {
                using var rawStream = System.IO.File.OpenRead(sourcePath);
                ExtractSelectedSuperPartitions(rawStream, outputDir, selectedNameSet);
            }
        });
    }

    private static void ExtractSelectedSuperPartitions(Stream superStream, string outputDir, HashSet<string> selectedNameSet)
    {
        var metadataReader = new MetadataReader();
        var metadata = metadataReader.ReadFromImageStream(superStream);

        foreach (var partition in metadata.Partitions)
        {
            var name = partition.GetName();
            if (!selectedNameSet.Contains(name))
            {
                continue;
            }

            var outputPath = Path.Combine(outputDir, $"{name}.img");

            ulong totalSectors = 0;
            for (var i = 0; i < partition.NumExtents; i++)
            {
                totalSectors += metadata.Extents[(int)(partition.FirstExtentIndex + i)].NumSectors;
            }

            var totalSize = (long)totalSectors * MetadataFormat.LP_SECTOR_SIZE;
            using var outFs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            outFs.SetLength(totalSize);

            long currentOutOffset = 0;
            for (var i = 0; i < partition.NumExtents; i++)
            {
                var extent = metadata.Extents[(int)(partition.FirstExtentIndex + i)];
                var size = (long)extent.NumSectors * MetadataFormat.LP_SECTOR_SIZE;

                if (extent.TargetType == MetadataFormat.LP_TARGET_TYPE_LINEAR)
                {
                    var sourceOffset = (long)extent.TargetData * MetadataFormat.LP_SECTOR_SIZE;
                    superStream.Seek(sourceOffset, SeekOrigin.Begin);
                    outFs.Seek(currentOutOffset, SeekOrigin.Begin);
                    CopyStreamPart(superStream, outFs, size);
                }

                currentOutOffset += size;
            }
        }
    }

    private static void CopyStreamPart(Stream input, Stream output, long length)
    {
        var buffer = new byte[1024 * 1024];
        var remaining = length;
        while (remaining > 0)
        {
            var toRead = (int)Math.Min(buffer.Length, remaining);
            var read = input.Read(buffer, 0, toRead);
            if (read <= 0)
            {
                break;
            }

            output.Write(buffer, 0, read);
            remaining -= read;
        }
    }

    // general helper accepting an explicit partition name filter (null/empty = all)
    private static async Task ExtractPayloadUrlSelectedAsync(string sourceUrl, string outputDir, string[]? partitionNames)
    {
        await PayloadParser.ExtractSelectedPartitionsFromUrlV2Async(sourceUrl, outputDir, partitionNames);
    }

    // old overload kept for compatibility with existing callers
    private static Task ExtractPayloadUrlSelectedAsync(string sourceUrl, string outputDir, List<FalshPartModel> selectedParts)
    {
        var names = selectedParts
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return ExtractPayloadUrlSelectedAsync(sourceUrl, outputDir, names);
    }

    /// <summary>
    /// <para>从 NTPI 文件中提取选中的分区镜像。</para>
    /// Extracts selected partition images from the NTPI file.
    /// </summary>
    private static async Task ExtractNtpiSelectedAsync(string sourcePath, string outputDir, List<FalshPartModel> selectedParts)
    {
        var names = selectedParts
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        await Task.Run(() =>
        {
            var reader = new NtpiReader(new FirmwareKit.NTPi.Crypto.AesCbcCryptoProvider(), new FirmwareKit.NTPi.Compression.Lzma2Compressor());
            reader.Unpack(sourcePath, outputDir);
        });

        // If specific partitions were selected, remove unselected .img files
        if (names.Count > 0)
        {
            foreach (var file in Directory.GetFiles(outputDir, "*.img"))
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (!names.Contains(fileName))
                {
                    try { System.IO.File.Delete(file); } catch { }
                }
            }
        }
    }

    /// <summary>
    /// <para>从 NB0 文件中提取选中的分区镜像。</para>
    /// Extracts selected partition images from the NB0 file.
    /// </summary>
    private static async Task ExtractNb0SelectedAsync(string sourcePath, string outputDir, List<FalshPartModel> selectedParts)
    {
        var names = selectedParts
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        await Task.Run(() =>
        {
            var extractor = new Nb0Extractor();
            extractor.Extract(sourcePath, outputDir);
        });

        // If specific partitions were selected, remove unselected files
        if (names.Count > 0)
        {
            foreach (var file in Directory.GetFiles(outputDir))
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (!names.Contains(fileName))
                {
                    try { System.IO.File.Delete(file); } catch { }
                }
            }
        }
    }

    private static string GetUnpackOutputDir(string sourcePath, bool isUrl)
    {
        if (isUrl)
        {
            var uri = new Uri(sourcePath, UriKind.Absolute);
            var name = Path.GetFileName(Uri.UnescapeDataString(uri.AbsolutePath));
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "payload_url";
            }

            return Path.Combine(Global.backup_path, $"{name}-unpack");
        }

        var parentDir = Path.GetDirectoryName(sourcePath) ?? Directory.GetCurrentDirectory();
        return Path.Combine(parentDir, $"{Path.GetFileName(sourcePath)}-unpack");
    }

    private void TrySyncFileNamesFromUnpackFolder(string sourcePath, bool isUrl = false)
    {
        var unpackDir = GetUnpackOutputDir(sourcePath, isUrl);
        if (!Directory.Exists(unpackDir))
        {
            return;
        }

        var vm = GetViewModel();
        if (vm.FalshPartModel.Count == 0)
        {
            AdvancedflashLog.Text += $"\nDetected unpack folder: {unpackDir}";
            return;
        }

        var existingImages = new HashSet<string>(
            Directory.GetFiles(unpackDir, "*.img", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!),
            StringComparer.OrdinalIgnoreCase
        );

        var matched = 0;
        foreach (var item in vm.FalshPartModel)
        {
            var expectedImage = $"{item.Name}.img";
            if (existingImages.Contains(expectedImage))
            {
                item.FileName = expectedImage;
                item.FullFilePath = Path.Combine(unpackDir, expectedImage);
                matched++;
            }
        }

        if (matched > 0)
        {
            AdvancedflashLog.Text += $"\nAuto-matched {matched} extracted images from {Path.GetFileName(unpackDir)}";
        }
    }

    private async void SetAll(object sender, RoutedEventArgs args)
    {
        if (SelectAll.IsChecked == true)
        {
            foreach (var item in GetViewModel().FalshPartModel)
            {
                if (!item.SelectDis == false)
                {
                    if (item.Name.Contains("crc") && UotanToolbox.Settings.Default.UseNative)
                    {
                        Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Warn")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_CantCRC")).Dismiss().ByClickingBackground().TryShow();
                        continue;
                    }
                    item.Select = true;
                }
            }
        }
        else
        {
            foreach (var item in GetViewModel().FalshPartModel)
            {
                item.Select = false;
            }
        }
        int total = 0;
        foreach (var item in GetViewModel().FalshPartModel)
        {
            if (item.Select == true)
            {
                total++;
            }
        }
        Total.Text = total.ToString();
    }

    private async void Selected(object sender, RoutedEventArgs args)
    {
        if (sender is CheckBox checkBox && checkBox.DataContext is FalshPartModel item)
        {
            await Task.Delay(100);
            if (item.Name.Contains("crc") && UotanToolbox.Settings.Default.UseNative && item.Select == true)
            {
                // 强制取消选中
                checkBox.IsChecked = false;
                item.Select = false;
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Warn"))
                    .OfType(NotificationType.Error)
                    .WithContent(GetTranslation("Advancedflash_CantCRC"))
                    .Dismiss().ByClickingBackground().TryShow();
            }
        }

        await Task.Delay(100);
        int total = 0;
        foreach (var part in GetViewModel().FalshPartModel)
        {
            if (part.Select == true)
            {
                total++;
            }
        }
        Total.Text = total.ToString();
    }

    private async void OpenImageFile(object sender, RoutedEventArgs args)
    {
        if (sender is not Button button || button.DataContext is not FalshPartModel falshPartModel)
        {
            return;
        }

        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
        {
            return;
        }

        System.Collections.Generic.IReadOnlyList<IStorageFile> file = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });
        if (file.Count >= 1)
        {
            var path = StringHelper.FilePath(file[0].Path.ToString() ?? string.Empty);
            falshPartModel.FileName = Path.GetFileName(path);
            falshPartModel.FullFilePath = path;
            if (System.IO.File.Exists(path))
            {
                falshPartModel.Size = StringHelper.byte2AUnit((ulong)new System.IO.FileInfo(path).Length);
            }
        }
    }

    private async void ReadInfo(object sender, RoutedEventArgs args)
    {
        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        if (sukiViewModel.Status == "Fastboot")
        {
            SetEnabled(true);
            Global.checkdevice = false;
            var vm = GetViewModel();
            vm.FalshPartModel.Clear();
            string allinfo = await FeaturesHelper.FastbootCmd(Global.thisdevice, "getvar all");
            string[] parts = new string[1000];
            string[] allinfos = allinfo.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < allinfos.Length; i++)
            {
                if (allinfos[i].Contains("partition-size"))
                {
                    parts[i] = allinfos[i];
                }
            }
            parts = [.. parts.Where(s => !string.IsNullOrEmpty(s))];
            PartModel[] part = new PartModel[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                string[] partinfos = parts[i].Split([':', ' '], StringSplitOptions.RemoveEmptyEntries);
                string size = StringHelper.byte2AUnit((ulong)Convert.ToInt64(partinfos[3].Replace("0x", ""), 16));
                vm.FalshPartModel.Add(new FalshPartModel
                {
                    Select = false,
                    Name = partinfos[2],
                    Size = size,
                    Command = "",
                    FileName = "选择文件"
                });
            }
            SetEnabled(false);
            Global.checkdevice = true;
        }
        else if (sukiViewModel.Status == "Fastbootd")
        {
            SetEnabled(true);
            Global.checkdevice = false;
            var vm = GetViewModel();
            vm.FalshPartModel.Clear();
            string allinfo = await FeaturesHelper.FastbootCmd(Global.thisdevice, "getvar all");
            string[] parts = new string[1000];
            string[] allinfos = allinfo.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            if ((bool?)ShowAllPart?.IsChecked == true)
            {
                for (int i = 0; i < allinfos.Length; i++)
                {
                    if (allinfos[i].Contains("partition-size"))
                    {
                        parts[i] = allinfos[i];
                    }
                }
                parts = [.. parts.Where(s => !string.IsNullOrEmpty(s))];
                PartModel[] part = new PartModel[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                {
                    string[] partinfos = parts[i].Split([':', ' '], StringSplitOptions.RemoveEmptyEntries);
                    string size = StringHelper.byte2AUnit((ulong)Convert.ToInt64(partinfos[3].Replace("0x", ""), 16));
                    vm.FalshPartModel.Add(new FalshPartModel
                    {
                        Select = false,
                        Name = partinfos[2],
                        Size = size,
                        Command = "",
                        FileName = "选择文件"
                    });
                }
            }
            else
            {
                for (int i = 0; i < allinfos.Length; i++)
                {
                    if (allinfos[i].Contains("is-logical") && allinfos[i].Contains("yes"))
                    {
                        string[] vpartinfos = allinfos[i].Split([':', ' '], StringSplitOptions.RemoveEmptyEntries);
                        parts[i] = vpartinfos[2];
                    }
                }
                parts = [.. parts.Where(s => !string.IsNullOrEmpty(s))];
                PartModel[] vpart = new PartModel[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                {
                    for (int j = 0; j < allinfos.Length; j++)
                    {
                        if (allinfos[j].Contains("partition-size"))
                        {
                            string[] partinfos = allinfos[j].Split([':', ' '], StringSplitOptions.RemoveEmptyEntries);
                            if (partinfos[2] == parts[i])
                            {
                                string size = StringHelper.byte2AUnit((ulong)Convert.ToInt64(partinfos[3].Replace("0x", ""), 16));
                                vm.FalshPartModel.Add(new FalshPartModel
                                {
                                    Select = false,
                                    Name = partinfos[2],
                                    Size = size,
                                    Command = "",
                                    FileName = "选择文件"
                                });
                            }
                        }
                    }
                }
            }
            SetEnabled(false);
            Global.checkdevice = true;
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void FlashSelect(object sender, RoutedEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(File.Text))
        {
            AdvancedflashLog.Text += "\nInvalid selected file path.";
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_SelectTip")).Dismiss().ByClickingBackground().TryShow();
            return;
        }

        var vm = GetViewModel();
        var selectedParts = vm.FalshPartModel.Where(x => x.Select).ToList();
        if (selectedParts.Count == 0)
        {
            AdvancedflashLog.Text += "\nNo partition selected.";
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_NoPart")).Dismiss().ByClickingBackground().TryShow();
            return;
        }

        // OZIP/OPS/OFP/NB0/NTPI formats only support extraction, not direct flashing
        if (_parsedFileType is ParsedFileType.Ozip or ParsedFileType.Ops or ParsedFileType.Ofp
            or ParsedFileType.Nb0 or ParsedFileType.Ntpi)
        {
            AdvancedflashLog.Text += "\nThis format does not support direct flashing. Please extract first.";
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Warn")).OfType(NotificationType.Warning).WithContent("This format only supports extraction, not direct flashing.").Dismiss().ByClickingBackground().TryShow();
            return;
        }

        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        if (sukiViewModel.Status == "Fastboot" || sukiViewModel.Status == "Fastbootd")
        {
            string path = File.Text;
            Global.checkdevice = false;
            AdvancedflashLog.Text = "";
            output = "";
            SetEnabled(true);

            var extension = Path.GetExtension(path);
            if (ScriptList.Contains(extension, StringComparer.OrdinalIgnoreCase) || Directory.Exists(path))
            {
                _parsedFileType = ParsedFileType.Script;
                AdvancedflashLog.Text += $"\nSkip script type: {Path.GetFileName(path)}";
                if (DelCow.IsChecked == true)
                {
                    string cow = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} getvar all");
                    string[] cowparts = FeaturesHelper.GetVPartList(cow);
                    for (int i = 0; i < cowparts.Length; i++)
                    {
                        if (cowparts[i].Contains("-cow"))
                        {
                            await Fastboot($"-s {Global.thisdevice} delete-logical-partition {cowparts[i]}");
                        }
                        FileHelper.Write(fastboot_log_path, output);
                        if (output.Contains("FAILED") || output.Contains("error"))
                        {
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
                            SetEnabled(false);
                            Global.checkdevice = true;
                            return;
                        }
                    }
                }
                //检测后的脚本逻辑写这里
                foreach (var item in vm.FalshPartModel)
                {
                    if (item.Select == true)
                    {
                        string slot = "";
                        string active = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} getvar current-slot");
                        if (active.Contains("current-slot: a"))
                        {
                            slot = "_a";
                        }
                        else if (active.Contains("current-slot: b"))
                        {
                            slot = "_b";
                        }
                        else
                        {
                            slot = null;
                        }
                        if (item.Name.Contains("crc") && UotanToolbox.Settings.Default.UseNative)
                        {
                            continue;
                        }
                        if (item.Command == "create")
                        {
                            await Fastboot($"-s {Global.thisdevice} create-logical-partition {item.Name}{slot} 00");
                        }
                        else if (item.Command == "delete")
                        {
                            await Fastboot($"-s {Global.thisdevice} delete-logical-partition {item.Name}{slot}");
                        }
                        else if (item.Name == "super_empty")
                        {
                            await Fastboot($"-s {Global.thisdevice} wipe-super {item.FullFilePath}");
                        }
                        else if (Path.GetExtension(item.FullFilePath) == ".zst")
                        {
                            AdvancedflashLog.Text += GetTranslation("Wiredflash_ZST");
                            var zstfile = System.IO.File.OpenRead(item.FullFilePath);
                            string zstname = Path.GetFileNameWithoutExtension(item.FullFilePath);
                            if (!zstname.Contains(".img"))
                            {
                                zstname = zstname + ".img";
                            }
                            string outfile = Path.Combine(Path.GetDirectoryName(item.FullFilePath), zstname);
                            var zstout = System.IO.File.OpenWrite(outfile);
                            var decompress = new DecompressionStream(zstfile);
                            await decompress.CopyToAsync(zstout);
                            decompress.Close();
                            zstout.Close();
                            zstfile.Close();
                            await Fastboot($"-s {Global.thisdevice} flash {item.Name} {outfile}");
                        }
                        else if (item.Name.Contains("vbmeta") && DisVbmeta.IsChecked == true)
                        {
                            await Fastboot($"-s {Global.thisdevice} {Global.VbmetaCommand} flash {item.Name} {item.FullFilePath}");
                        }
                        else if (item.Name == Global.SetBoot && AddRoot != null && AddRoot.IsChecked == true && !string.IsNullOrEmpty(Global.MagiskAPKPath))
                        {
                            AdvancedflashLog.Text += GetTranslation("Wiredflash_RepairBoot");
                            Global.Bootinfo = await ImageDetect.Boot_Detect(item.FullFilePath);
                            Global.Zipinfo = await PatchDetect.Patch_Detect(Global.MagiskAPKPath);
                            string newboot = null;
                            switch (Global.Zipinfo.Mode)
                            {
                                case PatchMode.Magisk:
                                    newboot = await MagiskPatch.Magisk_Patch_Mouzei(Global.Zipinfo, Global.Bootinfo);
                                    break;
                                case PatchMode.GKI:
                                    newboot = await KernelSUPatch.GKI_Patch(Global.Zipinfo, Global.Bootinfo);
                                    break;
                                case PatchMode.LKM:
                                    newboot = await KernelSUPatch.LKM_Patch(Global.Zipinfo, Global.Bootinfo);
                                    break;
                            }
                            await Fastboot($"-s {Global.thisdevice} flash {Global.SetBoot} \"{newboot}\"");
                        }
                        else
                        {
                            await Fastboot($"-s {Global.thisdevice} {item.Command} {item.Name} {item.FullFilePath}");
                        }
                        FileHelper.Write(fastboot_log_path, output);
                        if (output.Contains("FAILED") || output.Contains("error"))
                        {
                            if (!Directory.Exists(path))
                            {
                                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
                                SetEnabled(false);
                                Global.checkdevice = true;
                                return;
                            }
                            AdvancedflashLog.Text += "\n注意！出现错误 Attention! An error occurred\n";
                        }
                    }
                }
                if (ErasData.IsChecked == true)
                {
                    await Fastboot($"-s {Global.thisdevice} erase metadata");
                    await Fastboot($"-s {Global.thisdevice} erase userdata");
                }
                Global.MainDialogManager.CreateDialog()
                                .WithTitle(GetTranslation("Common_Succ"))
                                .WithContent(GetTranslation("Wiredflash_ROMFlash"))
                                .OfType(NotificationType.Success)
                                .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ => await Fastboot($"-s {Global.thisdevice} reboot"), true)
                                .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                .TryShow();
                SetEnabled(false);
                Global.checkdevice = true;
                return;
            }

            try
            {
                var sourcePath = File.Text ?? string.Empty;
                var isUrl = Uri.IsWellFormedUriString(sourcePath, UriKind.Absolute);
                if (string.IsNullOrWhiteSpace(sourcePath))
                {
                    AdvancedflashLog.Text += "\nInvalid image path.";
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_SelectTip")).Dismiss().ByClickingBackground().TryShow();
                    return;
                }

                if (!isUrl && !System.IO.File.Exists(sourcePath))
                {
                    AdvancedflashLog.Text += "\nInvalid image path.";
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_SelectTip")).Dismiss().ByClickingBackground().TryShow();
                    return;
                }

                if (isUrl && _parsedFileType != ParsedFileType.PayloadUrl)
                {
                    AdvancedflashLog.Text += "\nCurrent URL is not in payload_url mode.";
                    Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_UrlNotPayload")).Dismiss().ByClickingBackground().TryShow();
                    return;
                }

                var extractParts = vm.FalshPartModel.Where(x => x.Select && string.IsNullOrWhiteSpace(x.FullFilePath)).ToList();
                var outputDir = GetUnpackOutputDir(sourcePath, isUrl);
                Directory.CreateDirectory(outputDir);
                if (extractParts.Count > 0)
                {
                    switch (_parsedFileType)
                    {
                        case ParsedFileType.Payload:
                            await ExtractPayloadSelectedAsync(sourcePath, outputDir, extractParts);
                            break;
                        case ParsedFileType.Super:
                            await ExtractSuperSelectedAsync(sourcePath, outputDir, extractParts);
                            break;
                        case ParsedFileType.PayloadUrl:
                            await ExtractPayloadUrlSelectedAsync(sourcePath, outputDir, extractParts.Select(x => x.Name).ToArray());
                            break;
                        case ParsedFileType.Ntpi:
                            await ExtractNtpiSelectedAsync(sourcePath, outputDir, extractParts);
                            break;
                        case ParsedFileType.Nb0:
                            await ExtractNb0SelectedAsync(sourcePath, outputDir, extractParts);
                            break;
                        case ParsedFileType.Ozip:
                        case ParsedFileType.Ops:
                        case ParsedFileType.Ofp:
                            await ExtractOppoSelectedAsync(sourcePath, outputDir, extractParts);
                            break;
                        default:
                            AdvancedflashLog.Text += "\nUnknown image type. Please re-open image file first.";
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_Unsupport")).Dismiss().ByClickingBackground().TryShow();
                            return;
                    }

                    var successCount = 0;
                    foreach (var item in extractParts)
                    {
                        var outPath = Path.Combine(outputDir, $"{item.Name}.img");
                        if (System.IO.File.Exists(outPath))
                        {
                            item.FileName = Path.GetFileName(outPath);
                            item.FullFilePath = outPath;
                            successCount++;
                        }
                    }
                    AdvancedflashLog.Text += $"\nExtract finished: {successCount}/{extractParts.Count} -> {outputDir}\nFlashing...\n";
                }

                Global.checkdevice = false;
                AdvancedflashLog.Text = "";
                output = "";
                if (DelCow.IsChecked == true)
                {
                    string cow = await CallExternalProgram.Fastboot($"-s {Global.thisdevice} getvar all");
                    string[] cowparts = FeaturesHelper.GetVPartList(cow);
                    for (int i = 0; i < cowparts.Length; i++)
                    {
                        if (cowparts[i].Contains("-cow"))
                        {
                            await Fastboot($"-s {Global.thisdevice} delete-logical-partition {cowparts[i]}");
                        }
                        FileHelper.Write(fastboot_log_path, output);
                        if (output.Contains("FAILED") || output.Contains("error"))
                        {
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
                            SetEnabled(false);
                            Global.checkdevice = true;
                            return;
                        }
                    }
                }
                foreach (var item in vm.FalshPartModel)
                {
                    if (item.Select == true)
                    {
                        if (item.Name.Contains("vbmeta") && DisVbmeta.IsChecked == true)
                        {
                            await Fastboot($"-s {Global.thisdevice} {Global.VbmetaCommand} flash {item.Name} {item.FullFilePath}");
                        }
                        else if (item.Name == Global.SetBoot && AddRoot != null && AddRoot.IsChecked == true && !string.IsNullOrEmpty(Global.MagiskAPKPath))
                        {
                            AdvancedflashLog.Text += GetTranslation("Wiredflash_RepairBoot");
                            Global.Bootinfo = await ImageDetect.Boot_Detect(item.FullFilePath);
                            Global.Zipinfo = await PatchDetect.Patch_Detect(Global.MagiskAPKPath);
                            string newboot = null;
                            switch (Global.Zipinfo.Mode)
                            {
                                case PatchMode.Magisk:
                                    newboot = await MagiskPatch.Magisk_Patch_Mouzei(Global.Zipinfo, Global.Bootinfo);
                                    break;
                                case PatchMode.GKI:
                                    newboot = await KernelSUPatch.GKI_Patch(Global.Zipinfo, Global.Bootinfo);
                                    break;
                                case PatchMode.LKM:
                                    newboot = await KernelSUPatch.LKM_Patch(Global.Zipinfo, Global.Bootinfo);
                                    break;
                            }
                            await Fastboot($"-s {Global.thisdevice} flash {Global.SetBoot} \"{newboot}\"");
                        }
                        else
                        {
                            await Fastboot($"-s {Global.thisdevice} flash {item.Name} {item.FullFilePath}");
                        }
                        FileHelper.Write(fastboot_log_path, output);
                        if (output.Contains("FAILED") || output.Contains("error"))
                        {
                            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
                            SetEnabled(false);
                            Global.checkdevice = true;
                            FileHelper.OpenFolder(Path.Combine(outputDir));
                            return;
                        }
                    }
                }
                if (ErasData.IsChecked == true)
                {
                    await Fastboot($"-s {Global.thisdevice} erase metadata");
                    await Fastboot($"-s {Global.thisdevice} erase userdata");
                }
                Global.checkdevice = true;
                Global.MainDialogManager.CreateDialog()
                                .WithTitle(GetTranslation("Common_Succ"))
                                .WithContent(GetTranslation("Wiredflash_ROMFlash"))
                                .OfType(NotificationType.Success)
                                .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ => await Fastboot($"-s {Global.thisdevice} reboot"), true)
                                .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                                .TryShow();
                FileHelper.OpenFolder(Path.Combine(outputDir));
            }
            catch (Exception ex)
            {
                AdvancedflashLog.Text += $"Error: {ex.Message}";
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
            }
            finally
            {
                SetEnabled(false);
                Global.checkdevice = true;
            }

            SetEnabled(false);
            Global.checkdevice = true;
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void EraseSelect(object sender, RoutedEventArgs args)
    {
        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        if (sukiViewModel.Status == "Fastboot" || sukiViewModel.Status == "Fastbootd")
        {
            SetEnabled(true);
            Global.MainDialogManager.CreateDialog()
                            .WithTitle(GetTranslation("Common_Warn"))
                            .WithContent(GetTranslation("Advancedflash_ClaerCof"))
                            .OfType(NotificationType.Success)
                            .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                            {
                                foreach (var item in GetViewModel().FalshPartModel)
                                {
                                    if (item.Select == true)
                                    {
                                        await Fastboot($"-s {Global.thisdevice} erase {item.Name}");
                                    }
                                }
                            }, true)
                            .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                            .TryShow();
            SetEnabled(false);
        }
        else
        {
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Common_EnterFastboot")).Dismiss().ByClickingBackground().TryShow();
        }
    }

    private async void SetOther(object sender, RoutedEventArgs args)
    {
        MainViewModel sukiViewModel = GlobalData.MainViewModelInstance;
        if (sukiViewModel.Status == GetTranslation("Home_Fastboot") || sukiViewModel.Status == GetTranslation("Home_Fastbootd"))
        {
            AdvancedflashLog.Text = "";
            string shell = string.Format($"-s {Global.thisdevice} set_active other");
            await Fastboot(shell);
        }
        else
        {
            Global.MainDialogManager.CreateDialog()
                                    .WithTitle(GetTranslation("Common_Error"))
                                    .OfType(NotificationType.Error)
                                    .WithContent(GetTranslation("Common_EnterFastboot"))
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
        }
    }

    private async void CheckRoot(object sender, RoutedEventArgs args)
    {
        Global.MainDialogManager.CreateDialog()
                                .WithViewModel(_ => new SetMagiskDialogViewModel())
                                .TryShow();
    }

    private async void SetCommand(object sender, RoutedEventArgs args)
    {
        Global.MainDialogManager.CreateDialog()
                            .WithViewModel(_ => new SetVbmetaDialogViewModel())
                            .TryShow();
    }

    private async void ToWiredPkg(object sender, RoutedEventArgs args)
    {
        SetEnabled(true);
        try
        {
            var sourcePath = File.Text ?? string.Empty;
            var isUrl = Uri.IsWellFormedUriString(sourcePath, UriKind.Absolute);
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                AdvancedflashLog.Text += "\nInvalid image path.";
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_SelectTip")).Dismiss().ByClickingBackground().TryShow();
                return;
            }

            if (!isUrl && !System.IO.File.Exists(sourcePath))
            {
                AdvancedflashLog.Text += "\nInvalid image path.";
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_SelectTip")).Dismiss().ByClickingBackground().TryShow();
                return;
            }

            if (isUrl || _parsedFileType == ParsedFileType.Super)
            {
                AdvancedflashLog.Text += GetTranslation("Advancedflash_NeedFullPkg");
                Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Advancedflash_NeedFullPkg")).Dismiss().ByClickingBackground().TryShow();
                return;
            }

            var vm = GetViewModel();
            var selectedParts = vm.FalshPartModel.ToList();

            var parentDir = Path.GetDirectoryName(sourcePath) ?? Directory.GetCurrentDirectory();
            var outtxtDir = Path.Combine(parentDir, $"UotanToolbox_{Path.GetFileName(sourcePath)}", "flash_fastbootd.txt");
            var outputDir = Path.Combine(parentDir, $"UotanToolbox_{Path.GetFileName(sourcePath)}", "images");
            Directory.CreateDirectory(outputDir);
            //await ExtractPayloadSelectedAsync(sourcePath, outputDir, selectedParts);

            var partlist = string.Join("\r\n", selectedParts.Select(x => x.Name).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray());
            FileHelper.Write(outtxtDir, partlist);
            FileHelper.OpenFolder(Path.Combine(outputDir));
            AdvancedflashLog.Text += GetTranslation("Common_Execution");
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("Common_Execution")).Dismiss().ByClickingBackground().TryShow();
        }
        catch (Exception ex)
        {
            AdvancedflashLog.Text += $"Error: {ex.Message}";
            Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Error")).OfType(NotificationType.Error).WithContent(GetTranslation("Wiredflash_FlashError")).Dismiss().ByClickingBackground().TryShow();
        }
        finally
        {
            SetEnabled(false);
        }
    }

    private async void Export(object sender, RoutedEventArgs args)
    {
        //string selectedType = ExportScr.SelectedItem as string;
        //if (!string.IsNullOrEmpty(selectedType))
        //{
        //    var vm = GetViewModel();
        //    if (vm.FalshPartModel == null || vm.FalshPartModel.Count == 0)
        //    {
        //        AdvancedflashLog.Text += "\nError: No partition data to export.";
        //        return;
        //    }

        //    TopLevel topLevel = TopLevel.GetTopLevel(this);
        //    IStorageFolder? startLocation = null;
        //    if (!string.IsNullOrEmpty(File.Text))
        //    {
        //        try
        //        {
        //            string path = File.Text;
        //            if (System.IO.File.Exists(path))
        //            {
        //                path = Path.GetDirectoryName(path);
        //            }
        //            if (Directory.Exists(path))
        //            {
        //                startLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(path);
        //            }
        //        }
        //        catch
        //        {
        //            // Ignore errors if the path is invalid
        //        }
        //    }

        //    var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        //    {
        //        Title = "Save Script File",
        //        DefaultExtension = selectedType.TrimStart('.'),
        //        SuggestedFileName = "flash_all" + selectedType,
        //        SuggestedStartLocation = startLocation,
        //        FileTypeChoices = new[]
        //        {
        //            new FilePickerFileType($"{selectedType} File") { Patterns = new[] { $"*{selectedType}" } }
        //        }
        //    });

        //    if (file != null)
        //    {
        //        try
        //        {
        //            using var stream = await file.OpenWriteAsync();
        //            using var writer = new StreamWriter(stream);

        //            if (selectedType == ".bat")
        //            {
        //                foreach (var part in vm.FalshPartModel)
        //                {
        //                    if (part.Command == "set_active")
        //                    {
        //                        await writer.WriteLineAsync($"fastboot %* set_active {part.FileName}");
        //                    }
        //                    else if (part.Command == "reboot")
        //                    {
        //                        await writer.WriteLineAsync("fastboot %* reboot");
        //                    }
        //                    else if (part.Command == "erase")
        //                    {
        //                        await writer.WriteLineAsync($"fastboot %* erase {part.Name}");
        //                    }
        //                    else
        //                    {
        //                        // 默认补全为 flash，没有文件名时按 images/分区名.img
        //                        string fName = string.IsNullOrEmpty(part.FileName) ? $"images/{part.Name}.img" : part.FileName;
        //                        fName = fName.Replace('/', '\\'); // .bat 使用反斜杠
        //                        string command = string.IsNullOrEmpty(part.Command) ? "flash" : part.Command;
        //                        await writer.WriteLineAsync($"fastboot %* {command} {part.Name} %~dp0{fName}");
        //                    }
        //                }
        //            }
        //            else if (selectedType == ".sh")
        //            {
        //                foreach (var part in vm.FalshPartModel)
        //                {
        //                    if (part.Command == "set_active")
        //                    {
        //                        await writer.WriteLineAsync($"fastboot $* set_active {part.FileName}");
        //                    }
        //                    else if (part.Command == "reboot")
        //                    {
        //                        await writer.WriteLineAsync("fastboot $* reboot");
        //                    }
        //                    else if (part.Command == "erase")
        //                    {
        //                        await writer.WriteLineAsync($"fastboot $* erase {part.Name}");
        //                    }
        //                    else
        //                    {
        //                        string fName = string.IsNullOrEmpty(part.FileName) ? $"images/{part.Name}.img" : part.FileName;
        //                        fName = fName.Replace('\\', '/'); // .sh 使用斜杠
        //                        string command = string.IsNullOrEmpty(part.Command) ? "flash" : part.Command;
        //                        await writer.WriteLineAsync($"fastboot $* {command} {part.Name} `dirname $0`/{fName}");
        //                    }
        //                }
        //            }
        //            else if (selectedType == ".txt")
        //            {
        //                foreach (var part in vm.FalshPartModel)
        //                {
        //                    if (part.Command == "delete" || part.Command == "create")
        //                    {
        //                        await writer.WriteLineAsync($"{part.Name} {part.Command}");
        //                    }
        //                    else if (part.Command == "erase" || part.Command == "set_active" || part.Command == "reboot")
        //                    {
        //                        // .txt 规范主要用于刷入或创建删除，跳过或记录为注释
        //                        await writer.WriteLineAsync($"# {part.Command} {part.Name} {part.FileName}");
        //                    }
        //                    else
        //                    {
        //                        // 默认镜像只写分区名，指定镜像写分区名和相对路径
        //                        if (string.IsNullOrEmpty(part.FileName) || part.FileName.Equals($"{part.Name}.img", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            await writer.WriteLineAsync(part.Name);
        //                        }
        //                        else
        //                        {
        //                            string relativePath = $"images/{part.FileName}".Replace('\\', '/');
        //                            await writer.WriteLineAsync($"{part.Name} {relativePath}");
        //                        }
        //                    }
        //                }
        //            }

        //            AdvancedflashLog.Text += $"\nExported successfully to {file.Name}";

        //            // 自动打开导出文件所在的目录
        //            if (file.TryGetLocalPath() is string localPath && !string.IsNullOrEmpty(localPath))
        //            {
        //                string directory = Path.GetDirectoryName(localPath);
        //                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
        //                {
        //                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        //                    {
        //                        FileName = directory,
        //                        UseShellExecute = true,
        //                        Verb = "open"
        //                    });
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            AdvancedflashLog.Text += $"\nExport Error: {ex.Message}";
        //        }
        //    }
        //}
    }
}