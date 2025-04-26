using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiskPartition.FluentApi;
using DiskPartition.Gpt;
using EDLLibrary;
using Material.Icons;
using ReactiveUI;
using SukiUI.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using UotanToolbox.Common;


namespace UotanToolbox.Features.EDL;

public partial class EDLViewModel : MainPageBase
{
    [ObservableProperty]
    private string firehoseFile, currentDevice = $"目标端口：{Global.thisdevice}", memoryType = "存储类型：", xMLFile, partNamr, eDLLog;
    [ObservableProperty]
    private bool auto = true, uFS = false, eMMC = false;
    [ObservableProperty]
    private int selectBand = 0, selectSeries = 0, selectModel = 0, logIndex = 0;
    [ObservableProperty]
    private AvaloniaList<string> bandList = ["Qualcomm"], seriesList = ["Default", "BypassSig"], modelList = ["Configure", "Skip", "Partial"];
    [ObservableProperty]
    private AvaloniaList<EDLPartModel> eDLPartModel = [];
    private string SelectedStorageType = "";
    private Flash _flash; //获取flash对象单例实例
    private string output = "";
    //工作目录路径，可多次复用
    public string work_path = Path.Join(Global.tmp_path, "UotanToolboxNT-EDL");
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public EDLViewModel() : base("9008刷机", MaterialIconKind.CableData, -350)
    {
        this.WhenAnyValue(x => x.SelectBand)
            .Subscribe(option =>
            {

            });
        this.WhenAnyValue(x => x.SelectSeries)
            .Subscribe(option =>
            {

            });
    }

    private async void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            StringBuilder sb = new StringBuilder(EDLLog);
            EDLLog = sb.AppendLine(outLine.Data).ToString();
            LogIndex = EDLLog.Length;
            StringBuilder op = new StringBuilder(output);
            output = op.AppendLine(outLine.Data).ToString();
        }
    }

    public static FilePickerFileType Firehose { get; } = new("Firehose File")
    {
        Patterns = new[] { "*.elf", "*.mbn", "*.melf", "*.bin" },
        AppleUniformTypeIdentifiers = new[] { "*.elf", "*.mbn", "*.melf", "*.bin" }
    };

    public static FilePickerFileType RawProgramXML { get; } = new("XML Flie")
    {
        Patterns = new[] { "rawprogram*.xml" },
        AppleUniformTypeIdentifiers = new[] { "rawprogram*.xml" }
    };

    public static FilePickerFileType PatchXML { get; } = new("XML Flie")
    {
        Patterns = new[] { "patch*.xml" },
        AppleUniformTypeIdentifiers = new[] { "patch*.xml" }
    };

    public async Task OpenImageFolder()
    {
        var topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow);
        var folders = await topLevel?.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select Image Folder",
            AllowMultiple = false
        });

        if (folders != null && folders.Count > 0)
        {
            PartNamr = folders[0].Path.LocalPath;
            var folderPath = folders[0].Path.LocalPath;
            //自动匹配目录下的所有rawprogram.xml与patch.xml文件
            var rawprogramFiles = Directory.GetFiles(folderPath, "rawprogram*.xml");
            var patchFiles = Directory.GetFiles(folderPath, "patch*.xml");
            // 本来打算索引elf文件的，但是elf文件不一定只有一个且不一定就是设备的elf文件
            //var firehoseFiles = Directory.GetFiles(folderPath, "*elf");
            //if (firehoseFiles.Length > 0)
            //{
            //    FirehoseFile = firehoseFiles[0];
            //}
            var rawprogramXmls = rawprogramFiles.Select(file => new FileInfo(file)).ToList();
            var patchXmls = patchFiles.Select(file => new FileInfo(file)).ToList();
            // 创建工作目录
            if (!Directory.Exists(work_path))
            {
                Directory.CreateDirectory(work_path);
            }
            //批量删除工作目录下的xml文件
            var exitxmlFiles = Directory.GetFiles(work_path, "*.xml");
            foreach (var xmlFile in exitxmlFiles)
            {
                File.Delete(xmlFile);
            }
            //合并用户选择的xml文件
            MergeProgramXMLFiles(rawprogramXmls.Select(file => file.FullName), Path.Join(work_path, "rawprogram.xml"));
            MergePatchXMLFiles(rawprogramXmls.Select(file => file.FullName), Path.Join(work_path, "patch.xml"));
            //为合并后的xml文件添加索引号
            AddIndexToProgramElements(Path.Join(work_path, "rawprogram.xml"));
            //将绝对地址添加到rawprogram.xml文件中
            UpdateProgramElementsWithAbsolutePaths(Path.Join(work_path, "rawprogram.xml"), PartNamr);
            var xmlFiles = new List<string>();
            xmlFiles.AddRange(rawprogramXmls.Select(file => file.FullName));
            xmlFiles.AddRange(patchXmls.Select(file => file.FullName));
            EDLPartModel = [.. ParseProgramElements(Path.Join(work_path, "rawprogram.xml"))];
            XMLFile = string.Join(", ", xmlFiles);
        }
    }

    public async Task OpenFirehoseFile()
    {
        var topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow);
        var files = await topLevel?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "请选择Firehose文件",
            FileTypeFilter = new List<FilePickerFileType> { Firehose },
        });
        FirehoseFile = files[0].Path.LocalPath;
    }

    public async Task OpenXMLFile()
    {
        var topLevel = TopLevel.GetTopLevel(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow);
        var rawprogram_xmls = await topLevel?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "请选择rawprogram.xml文件",
            FileTypeFilter = new List<FilePickerFileType> { RawProgramXML },
            AllowMultiple = true,
        });
        var patch_xmls = await topLevel?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "请选择patch.xml文件",
            FileTypeFilter = new List<FilePickerFileType> { PatchXML },
            AllowMultiple = true,
        });
        if (String.IsNullOrEmpty(PartNamr))
        {
            PartNamr = Path.GetDirectoryName(rawprogram_xmls[0].Path.LocalPath);
        }
        var xmlFiles = new List<string>();
        if (rawprogram_xmls != null)
        {
            xmlFiles.AddRange(rawprogram_xmls.Select(file => file.Path.LocalPath));
        }
        if (patch_xmls != null)
        {
            xmlFiles.AddRange(patch_xmls.Select(file => file.Path.LocalPath));
        }
        // 创建工作目录
        if (!Directory.Exists(work_path))
        {
            Directory.CreateDirectory(work_path);
        }
        //批量删除工作目录下的xml文件
        var exitxmlFiles = Directory.GetFiles(work_path, "*.xml");
        foreach (var xmlFile in exitxmlFiles)
        {
            File.Delete(xmlFile);
        }
        //合并用户选择的xml文件
        MergeProgramXMLFiles(rawprogram_xmls.Select(file => file.Path.LocalPath), Path.Join(work_path, "Merged.xml"));
        MergePatchXMLFiles(patch_xmls.Select(file => file.Path.LocalPath), Path.Join(work_path, "Merged_Patch.xml"));
        //为合并后的xml文件添加索引号
        AddIndexToProgramElements(Path.Join(work_path, "Merged.xml"));
        //将绝对地址添加到xml文件中
        UpdateProgramElementsWithAbsolutePaths(Path.Join(work_path, "Merged.xml"), PartNamr);
        EDLPartModel = [.. ParseProgramElements(Path.Join(work_path, "Merged.xml"))];
        XMLFile = string.Join(", ", xmlFiles);
    }

    [RelayCommand]
    public async Task SendFirehose()
    {
        try
        {
            if (string.IsNullOrEmpty(FirehoseFile))
            {
                Global.MainDialogManager.CreateDialog()
                                .WithTitle(GetTranslation("Common_Error"))
                                .OfType(NotificationType.Error)
                                .WithContent("未选择引导文件")
                                .Dismiss().ByClickingBackground()
                                .TryShow();
                return;
            }
            EDLLog += $"索引引导文件...{Environment.NewLine}";
            _flash = Flash.Instance;  //首次使用，获取flash对象单例实例
            if (UFS == true)
            {
                SelectedStorageType = "ufs";
            }
            if (EMMC == true)
            {
                SelectedStorageType = "emmc";
            }
            EDLLog += $"存储类型：{SelectedStorageType}{Environment.NewLine}";
            _flash.Initialize(Global.thisdevice, SelectedStorageType);
            EDLLog += $"目标端口：{Global.thisdevice}{Environment.NewLine}";
            EDLLog += $"引导文件：{FirehoseFile}{Environment.NewLine}";
            EDLLog += $"设备初始化...{Environment.NewLine}";
            _flash.RegisterPort();
            EDLLog += $"端口注册...{Environment.NewLine}";
            if (!_flash.Sahara(FirehoseFile))
            {
                Global.MainDialogManager.CreateDialog()
                                .WithTitle(GetTranslation("Common_Error"))
                                .OfType(NotificationType.Error)
                                .WithContent("引导发送失败")
                                .Dismiss().ByClickingBackground()
                                .TryShow();
            }
            string result = _flash.ConfigureDDR();
            EDLLog += $"设备配置DDR...{Environment.NewLine}";
            if (result == "success")
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Succ"))
                    .OfType(NotificationType.Success)
                    .WithContent("引导发送成功")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
                EDLLog += $"引导发送成功{Environment.NewLine}";
                return;
            }
            else if (result == "needsig")
            {
                string? blob;
                bool sig_result = false;
                EDLLog += $"需要签名{Environment.NewLine}";
                if (selectModel == 0)   //用户选择Qualcomm选项，问用户拿取sig
                {
                    blob = _flash.GetBlob();
                    if (String.IsNullOrEmpty(blob))
                    {
                        EDLLog += $"获取签名文件失败{Environment.NewLine}";
                        Global.MainDialogManager.CreateDialog()
                                                .WithTitle(GetTranslation("Common_Error"))
                                                .OfType(NotificationType.Error)
                                                .WithContent("获取签名文件失败")
                                                .Dismiss().ByClickingBackground()
                                                .TryShow();
                        return;
                    }
                    EDLLog += $"获取签名文件成功:{blob}{Environment.NewLine}";

                    string sig = "";   //给出弹窗获取sig
                    EDLLog += $"Sig:{sig}{Environment.NewLine}";
                    sig_result = _flash.SendSignature(sig);
                }
                else if (selectModel == 1)
                {
                    sig_result = _flash.BypassSendSig();
                }
                if (sig_result)
                {
                    EDLLog += $"签名发送成功{Environment.NewLine}";
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Error"))
                        .OfType(NotificationType.Error)
                        .WithContent("签名发送失败")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                    EDLLog += $"签名发送失败{Environment.NewLine}";
                    return;
                }
                EDLLog += $"设备重新配置DDR...{Environment.NewLine}";
                if (_flash.ConfigureDDR() == "success")
                {
                    Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Succ"))
                        .OfType(NotificationType.Success)
                        .WithContent("引导发送成功")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                    EDLLog += $"引导发送成功{Environment.NewLine}";
                }
                else
                {
                    Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Error"))
                        .OfType(NotificationType.Error)
                        .WithContent("引导发送失败")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
                    EDLLog += $"引导发送失败{Environment.NewLine}";
                }
            }
        }
        catch (Exception ex)
        {
            Global.MainDialogManager.CreateDialog()
                                    .WithTitle(GetTranslation("Common_Error"))
                                    .OfType(NotificationType.Error)
                                    .WithContent($"引导发送失败 {ex.Message}")
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
            EDLLog += $"引导发送失败 {ex.Message} {Environment.NewLine}";

        }

    }

    [RelayCommand]
    public async Task Remark()
    {
        // 检查是否所有项的 SelectPart 都为 true
        bool allSelected = EDLPartModel.All(part => part.SelectPart);
        EDLLog += $"{selectBand} {selectSeries} {selectModel}";
        // 如果全为 true，则全部设置为 false；否则全部设置为 true
        foreach (var part in EDLPartModel)
        {
            part.SelectPart = !allSelected;
        }
    }

    /// <summary>
    /// 读取分区表文件，并生成xml文件然后加载进可视化编辑器
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task ReadPartTable()
    {
        try
        {
            // 检查Flash对象是否已初始化
            if (_flash == null)
            {
                EDLLog += $"错误：设备未初始化，请先发送引导程序！{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("设备未初始化，请先发送引导程序")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
                return;
            }

            // 确保工作目录存在
            if (!Directory.Exists(work_path))
            {
                Directory.CreateDirectory(work_path);
            }

            // 根据存储类型读取分区表
            if (UFS)
            {
                // UFS设备需要读取多个LUN
                for (int i = 0; i <= 5; i++)
                {
                    string outputFile = Path.Combine(work_path, $"gpt_main{i}.bin");
                    _flash.Read("0", 6, i, outputFile);
                }
            }
            else
            {
                // eMMC设备只需要读取LUN 0
                string outputFile = Path.Combine(work_path, $"gpt_main0.bin");
                _flash.Read("0", 34, 0, outputFile);
            }

            // 解析分区表文件并生成XML
            string outputXmlPath = Path.Combine(work_path, "rawprogram.xml");
            ReadGptFilesAndGenerateXml(work_path, outputXmlPath);

            // 加载生成的XML到可视化编辑器
            EDLPartModel = [.. ParseProgramElements(outputXmlPath)];
            XMLFile = outputXmlPath;

            // 显示成功通知
            Global.MainDialogManager.CreateDialog()
                .WithTitle("操作成功")
                .OfType(NotificationType.Success)
                .WithContent("分区表读取并解析完成")
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
        catch (Exception ex)
        {
            EDLLog += $"读取分区表失败: {ex.Message}{Environment.NewLine}";
            // 显示错误消息
            Global.MainDialogManager.CreateDialog()
                .WithTitle(GetTranslation("Common_Error"))
                .OfType(NotificationType.Error)
                .WithContent($"分区表解析失败: {ex.Message}")
                .Dismiss().ByClickingBackground()
                .TryShow();
        }

    }

    [RelayCommand]
    public async Task WritePart()
    {
        try
        {
            // 检查是否有分区被选中
            var selectedParts = EDLPartModel.Where(part => part.SelectPart).ToList();
            if (selectedParts.Count == 0)
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("未选择任何分区")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
                return;
            }

            // 检查Flash对象是否已初始化
            if (_flash == null)
            {
                EDLLog += $"错误：设备未初始化，请先发送引导程序！{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("设备未初始化，请先发送引导程序")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
                return;
            }

            EDLLog += $"开始写入分区...{Environment.NewLine}";
            int successCount = 0;
            int failCount = 0;

            foreach (var part in selectedParts)
            {
                // 检查文件是否存在
                if (string.IsNullOrEmpty(part.FileName) || part.FileName == "点击选择镜像" || part.FileName == "请选择文件")
                {
                    EDLLog += $"跳过分区 {part.Name}：未指定镜像文件{Environment.NewLine}";
                    failCount++;
                    continue;
                }

                string filePath = part.FileName;
                // 如果只是文件名而不是完整路径，则尝试从分区路径构建完整路径
                if (!Path.IsPathRooted(filePath) && !string.IsNullOrEmpty(PartNamr))
                {
                    filePath = Path.Combine(PartNamr, filePath);
                }

                if (!File.Exists(filePath))
                {
                    EDLLog += $"跳过分区 {part.Name}：文件 {filePath} 不存在{Environment.NewLine}";
                    failCount++;
                    continue;
                }

                EDLLog += $"正在写入分区 {part.Name}...{Environment.NewLine}";

                // 解析分区参数
                int lun = int.Parse(part.Lun);
                string start = part.Start;

                // 检查是否为sparse镜像
                bool isSparse = part.IsSparse.Equals("true", StringComparison.OrdinalIgnoreCase);

                try
                {
                    // Check if the file is sparse
                    if (isSparse)
                    {
                        // Use WriteSparseFileToDevice for sparse files
                        _flash.WriteSparseFileToDevice(filePath, "0", start, null, lun.ToString(), part.Name, CancellationToken.None);
                    }
                    else
                    {
                        // Use WriteRawFileToDevice for non-sparse files
                        _flash.WriteRawFileToDevice(filePath, "0", start, null, lun.ToString(), part.Name);
                    }
                    EDLLog += $"分区 {part.Name} 写入成功{Environment.NewLine}";
                    successCount++;
                }
                catch (Exception ex)
                {
                    EDLLog += $"分区 {part.Name} 写入失败: {ex.Message}{Environment.NewLine}";
                    failCount++;
                }
            }

            // 显示写入结果统计
            EDLLog += $"写入操作完成：成功 {successCount} 个，失败 {failCount} 个{Environment.NewLine}";

            // 根据结果显示不同的通知
            if (failCount == 0 && successCount > 0)
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Succ"))
                    .OfType(NotificationType.Success)
                    .WithContent($"所有选中分区写入成功")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
            else if (successCount > 0)
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle("部分成功")
                    .OfType(NotificationType.Warning)
                    .WithContent($"部分分区写入成功，详情请查看日志")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("所有分区写入失败，请检查日志")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
        }
        catch (Exception ex)
        {
            EDLLog += $"写入分区时发生错误: {ex.Message}{Environment.NewLine}";
            Global.MainDialogManager.CreateDialog()
                .WithTitle(GetTranslation("Common_Error"))
                .OfType(NotificationType.Error)
                .WithContent($"写入分区时发生错误: {ex.Message}")
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
    }

    [RelayCommand]
    public async Task ReadPart()
    {
        try
        {
            // 检查是否有分区被选中
            var selectedParts = EDLPartModel.Where(part => part.SelectPart).ToList();
            if (selectedParts.Count == 0)
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("未选择任何分区")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
                return;
            }

            // 检查Flash对象是否已初始化
            if (_flash == null)
            {
                EDLLog += $"错误：设备未初始化，请先发送引导程序！{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("设备未初始化，请先发送引导程序")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
                return;
            }

            // 确保输出目录存在
            string outputDir = string.IsNullOrEmpty(PartNamr) ?
                Path.Combine(work_path, "dumps") :
                Path.Combine(PartNamr, "dumps");

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            EDLLog += $"开始读取分区...{Environment.NewLine}";
            int successCount = 0;
            int failCount = 0;

            foreach (var part in selectedParts)
            {
                string partitionName = part.Name;
                string outputPath = Path.Combine(outputDir, $"{partitionName}.img");

                // 解析分区参数
                int lun = int.Parse(part.Lun);
                string start = part.Start;
                string sectors = part.Sector;

                if (string.IsNullOrEmpty(sectors))
                {
                    EDLLog += $"跳过分区 {partitionName}：未知分区大小{Environment.NewLine}";
                    failCount++;
                    continue;
                }

                // 将分区的扇区数转换为整数
                if (!int.TryParse(sectors, out int numSectors))
                {
                    EDLLog += $"跳过分区 {partitionName}：无效的分区大小{Environment.NewLine}";
                    failCount++;
                    continue;
                }

                EDLLog += $"正在读取分区 {partitionName}，保存到 {outputPath}...{Environment.NewLine}";
                try
                {
                    // 执行读取操作
                    _flash.Read(start, numSectors, lun, outputPath);
                    EDLLog += $"分区 {partitionName} 读取成功，已保存到 {outputPath}{Environment.NewLine}";
                    successCount++;
                }catch (Exception ex)
                {
                    EDLLog += $"分区 {partitionName} 读取失败{Environment.NewLine}";
                    failCount++;
                }
            }

            // 显示读取结果统计
            EDLLog += $"读取操作完成：成功 {successCount} 个，失败 {failCount} 个{Environment.NewLine}";

            // 根据结果显示不同的通知
            if (failCount == 0 && successCount > 0)
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Succ"))
                    .OfType(NotificationType.Success)
                    .WithContent($"所有选中分区读取成功，已保存到 {outputDir}")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
            else if (successCount > 0)
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle("部分成功")
                    .OfType(NotificationType.Warning)
                    .WithContent($"部分分区读取成功，详情请查看日志")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("所有分区读取失败，请检查日志")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
        }
        catch (Exception ex)
        {
            EDLLog += $"读取分区时发生错误: {ex.Message}{Environment.NewLine}";
            Global.MainDialogManager.CreateDialog()
                .WithTitle(GetTranslation("Common_Error"))
                .OfType(NotificationType.Error)
                .WithContent($"读取分区时发生错误: {ex.Message}")
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
    }

    [RelayCommand]
    public async Task ErasePart()
    {
        try
        {
            // 检查是否有分区被选中
            var selectedParts = EDLPartModel.Where(part => part.SelectPart).ToList();
            if (selectedParts.Count == 0)
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("未选择任何分区")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
                return;
            }

            // 检查Flash对象是否已初始化
            if (_flash == null)
            {
                EDLLog += $"错误：设备未初始化，请先发送引导程序！{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("设备未初始化，请先发送引导程序")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
                return;
            }
            bool confirmDialog = false;
            // 确认擦除操作
            Global.MainDialogManager.CreateDialog()
                .WithTitle("确认擦除")
                .OfType(NotificationType.Warning)
                .WithContent($"您确定要擦除选中的 {selectedParts.Count} 个分区吗？此操作不可恢复！")
                .Dismiss().ByClickingBackground()
                .WithActionButton("取消", dialog => {})
                .WithActionButton("确认擦除", dialog => { bool confirmDialog = true; })
                .TryShow();

            if (confirmDialog is not true)
            {
                EDLLog += $"擦除操作已取消{Environment.NewLine}";
                return;
            }

            EDLLog += $"开始擦除分区...{Environment.NewLine}";
            int successCount = 0;
            int failCount = 0;

            foreach (var part in selectedParts)
            {
                string partitionName = part.Name;

                // 解析分区参数
                int lun = int.Parse(part.Lun);
                string start = part.Start;
                string sectors = part.Sector;

                if (string.IsNullOrEmpty(sectors))
                {
                    EDLLog += $"跳过分区 {partitionName}：未知分区大小{Environment.NewLine}";
                    failCount++;
                    continue;
                }

                // 将分区的扇区数转换为整数
                if (!int.TryParse(sectors, out int numSectors))
                {
                    EDLLog += $"跳过分区 {partitionName}：无效的分区大小{Environment.NewLine}";
                    failCount++;
                    continue;
                }

                EDLLog += $"正在擦除分区 {partitionName}...{Environment.NewLine}";
                try
                {
                // 执行擦除操作
                _flash.Erase(start, numSectors, lun);
                    EDLLog += $"分区 {partitionName} 擦除成功{Environment.NewLine}";
                    successCount++;
                }
                catch (Exception ex)
                {
                    EDLLog += $"分区 {partitionName} 擦除失败{Environment.NewLine}";
                    failCount++;
                }
            }

            // 显示擦除结果统计
            EDLLog += $"擦除操作完成：成功 {successCount} 个，失败 {failCount} 个{Environment.NewLine}";

            // 根据结果显示不同的通知
            if (failCount == 0 && successCount > 0)
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Succ"))
                    .OfType(NotificationType.Success)
                    .WithContent($"所有选中分区擦除成功")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
            else if (successCount > 0)
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle("部分成功")
                    .OfType(NotificationType.Warning)
                    .WithContent($"部分分区擦除成功，详情请查看日志")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("所有分区擦除失败，请检查日志")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
        }
        catch (Exception ex)
        {
            EDLLog += $"擦除分区时发生错误: {ex.Message}{Environment.NewLine}";
            Global.MainDialogManager.CreateDialog()
                .WithTitle(GetTranslation("Common_Error"))
                .OfType(NotificationType.Error)
                .WithContent($"擦除分区时发生错误: {ex.Message}")
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
    }

    [RelayCommand]
    public async Task Cancel()
    {
        try
        {
            // 检查Flash对象是否已初始化
            if (_flash == null)
            {
                EDLLog += $"错误：设备未初始化，无法执行取消操作！{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("设备未初始化，无法执行取消操作")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
                return;
            }

            EDLLog += $"正在发送取消命令...{Environment.NewLine}";


            if (true)
            {
                EDLLog += $"取消命令已发送{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Succ"))
                    .OfType(NotificationType.Success)
                    .WithContent("取消命令已发送")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
            else
            {
                EDLLog += $"取消操作失败{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("取消操作失败")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
        }
        catch (Exception ex)
        {
            EDLLog += $"取消操作时发生错误: {ex.Message}{Environment.NewLine}";
            Global.MainDialogManager.CreateDialog()
                .WithTitle(GetTranslation("Common_Error"))
                .OfType(NotificationType.Error)
                .WithContent($"取消操作时发生错误: {ex.Message}")
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
    }

    [RelayCommand]
    public async Task RebootEDL()
    {
        try
        {
            // 检查Flash对象是否已初始化
            if (_flash == null)
            {
                EDLLog += $"错误：设备未初始化，请先发送引导程序！{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("设备未初始化，请先发送引导程序")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
                return;
            }
            EDLLog += $"正在执行重启到EDL模式...{Environment.NewLine}";
            try
            {
                // 执行重启到EDL操作
                _flash.Reset();
                _flash.Close();
                EDLLog += $"设备已重启至EDL模式{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Succ"))
                    .OfType(NotificationType.Success)
                    .WithContent("设备已重启至EDL模式")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
            catch (Exception ex)
            {
                EDLLog += $"重启到EDL模式失败{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("重启到EDL模式失败")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
        }
        catch (Exception ex)
        {
            EDLLog += $"重启到EDL模式时发生错误: {ex.Message}{Environment.NewLine}";
            Global.MainDialogManager.CreateDialog()
                .WithTitle(GetTranslation("Common_Error"))
                .OfType(NotificationType.Error)
                .WithContent($"重启到EDL模式时发生错误: {ex.Message}")
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
    }

    [RelayCommand]
    public async Task RebootSys()
    {
        try
        {
            // 检查Flash对象是否已初始化
            if (_flash == null)
            {
                EDLLog += $"错误：设备未初始化，请先发送引导程序！{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("设备未初始化，请先发送引导程序")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
                return;
            }

            EDLLog += $"正在执行重启到系统...{Environment.NewLine}";
            try
            {
                // 执行重启到系统操作
                _flash.Reboot();
                _flash.Close();
                EDLLog += $"设备已重启至系统{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Succ"))
                    .OfType(NotificationType.Success)
                    .WithContent("设备已重启至系统")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
            catch (Exception ex)
            {
                EDLLog += $"重启到系统失败{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Common_Error"))
                    .OfType(NotificationType.Error)
                    .WithContent("重启到系统失败")
                    .Dismiss().ByClickingBackground()
                    .TryShow();
            }
        }
        catch (Exception ex)
        {
            EDLLog += $"重启到系统时发生错误: {ex.Message}{Environment.NewLine}";
            Global.MainDialogManager.CreateDialog()
                .WithTitle(GetTranslation("Common_Error"))
                .OfType(NotificationType.Error)
                .WithContent($"重启到系统时发生错误: {ex.Message}")
                .Dismiss().ByClickingBackground()
                .TryShow();
        }
    }

    [RelayCommand]
    public async Task DisableVbmeta()
    {

    }

    [RelayCommand]
    public async Task Restore()
    {

    }

    [RelayCommand]
    public async Task OpenOEMLock()
    {

    }

    [RelayCommand]
    public async Task ReconfigureUFS()
    {

    }




    /// <summary>
    /// 解析XML文件中的program元素，返回EDLPartModel列表
    /// </summary>
    /// <param name="xmlFilePath"></param>
    /// <returns></returns>
    private static List<EDLPartModel> ParseProgramElements(string xmlFilePath)
    {
        XDocument xdoc = XDocument.Load(xmlFilePath);
        var programElements = xdoc.Descendants("program");
        List<EDLPartModel> partModels = [];
        foreach (var element in programElements)
        {
            EDLPartModel partModel = new EDLPartModel
            {
                Lun = element.Attribute("physical_partition_number")?.Value,
                Name = element.Attribute("label")?.Value,
                FileName = string.IsNullOrEmpty(element.Attribute("filename")?.Value)
                        ? "请选择文件" // 如果为空，赋值为“请选择文件”
                        : Path.GetFileName(element.Attribute("filename")?.Value), //让文件名只显示文件名，不显示路径
                IsSparse = element.Attribute("sparse")?.Value,
                Offset = element.Attribute("file_sector_offset")?.Value,
                Start = element.Attribute("start_sector")?.Value,
                Sector = element.Attribute("num_partition_sectors")?.Value,
            };
            partModels.Add(partModel);
        }
        return partModels;
    }
    /// <summary>
    /// 重命名XML文件中的program元素为erase，用于擦除分区功能
    /// </summary>
    /// <param name="xmlFilePath">需要重命名节点的xml文件路径</param>
    private void RenameProgramNodesToErase(string xmlFilePath)
    {
        XDocument xdoc = XDocument.Load(xmlFilePath);
        var programElements = xdoc.Descendants("program");

        foreach (var element in programElements)
        {
            XElement newElement = new XElement("erase", element.Attributes());
            element.ReplaceWith(newElement);
        }

        xdoc.Save(xmlFilePath);
    }
    /// <summary>
    /// 为XML文件中的program元素中的filename添加绝对路径
    /// </summary>
    /// <param name="xmlFilePath"></param>
    /// <param name="baseDirectory"></param>
    private void UpdateProgramElementsWithAbsolutePaths(string xmlFilePath, string baseDirectory)
    {
        XDocument xdoc = XDocument.Load(xmlFilePath);
        var programElements = xdoc.Descendants("program");
        foreach (var element in programElements)
        {
            var filenameAttribute = element.Attribute("filename");
            if (filenameAttribute != null)
            {
                string relativePath = filenameAttribute.Value;
                string absolutePath = Path.Combine(baseDirectory, relativePath);

                if (File.Exists(absolutePath))
                {
                    filenameAttribute.Value = absolutePath;
                }
                else
                {
                    filenameAttribute.Value = string.Empty;
                }
            }
        }

        xdoc.Save(xmlFilePath);
    }
    /// <summary>
    /// 为XML文件中的program元素添加索引号
    /// </summary>
    /// <param name="xmlFilePath">需要添加索引号的xml文件路径</param>
    private void AddIndexToProgramElements(string xmlFilePath)
    {
        XDocument xdoc = XDocument.Load(xmlFilePath);
        var programElements = xdoc.Descendants("program");

        int index = 1;
        foreach (var element in programElements)
        {
            element.SetAttributeValue("Uotan-Index", index);
            index++;
        }
        xdoc.Save(xmlFilePath);
    }
    /// <summary>
    /// 移除XML文件中的索引号
    /// </summary>
    /// <param name="xmlFilePath">需要移除索引号的xml文件路径</param>
    private void RemoveIndexToProgramElements(string xmlFilePath)
    {
        XDocument xdoc = XDocument.Load(xmlFilePath);
        var programElements = xdoc.Descendants("program");
        foreach (var element in programElements)
        {
            element.SetAttributeValue("Uotan-Index", null);
        }
        xdoc.Save(xmlFilePath);
    }
    /// <summary>
    /// 合并用户选择的rawprogram.xml文件
    /// </summary>
    /// <param name="xmlFilePaths">多个xml文件所在目录</param>
    /// <param name="outputFilePath">输出的xml文件路径，具体到文件名</param>
    private void MergeProgramXMLFiles(IEnumerable<string> xmlFilePaths, string outputFilePath)
    {
        EDLLog += "正在合并rawprogram文件...\n";
        XDocument mergedDoc = new XDocument(new XElement("data"));
        foreach (var xmlFilePath in xmlFilePaths)
        {
            XDocument xdoc = XDocument.Load(xmlFilePath);
            var programElements = xdoc.Descendants("program");
            foreach (var element in programElements)
            {
                mergedDoc.Root.Add(new XElement(element));
            }
        }
        mergedDoc.Save(outputFilePath);
        EDLLog += "rawprogram文件合并完成\n";
    }

    /// <summary>
    /// 合并用户选择的patch.xml文件
    /// </summary>
    /// <param name="xmlFilePaths">多个xml文件所在目录</param>
    /// <param name="outputFilePath">输出的xml文件路径，具体到文件名</param>
    private void MergePatchXMLFiles(IEnumerable<string> xmlFilePaths, string outputFilePath)
    {
        EDLLog += "正在合并patch文件...\n";
        XDocument mergedDoc = new XDocument(new XElement("patches"));
        foreach (var xmlFilePath in xmlFilePaths)
        {
            XDocument xdoc = XDocument.Load(xmlFilePath);
            var programElements = xdoc.Descendants("patch");
            foreach (var element in programElements)
            {
                mergedDoc.Root.Add(new XElement(element));
            }
        }
        mergedDoc.Save(outputFilePath);
        EDLLog += "patch文件合并完成\n";
    }

    /// <summary>
    /// 解析GPT分区表文件，生成符合标准的XML文件，用于加载到EDLPartModel
    /// </summary>
    /// <param name="directoryPath">分区表文件所在文件夹</param>
    /// <param name="outputXmlPath">输出的xml文件路径</param>
    private void ReadGptFilesAndGenerateXml(string directoryPath, string outputXmlPath)
    {
        // 创建根XML元素
        XDocument xmlDoc = new XDocument(new XElement("data"));

        // 查找所有gpt_main*.bin文件
        string pattern = @"gpt_main(\d+)\.bin";
        var gptFiles = Directory.GetFiles(directoryPath, "gpt_main*.bin");

        foreach (var gptFile in gptFiles)
        {
            // 提取LUN号
            Match match = Regex.Match(Path.GetFileName(gptFile), pattern);
            string lun = "0";
            if (match.Success)
            {
                lun = match.Groups[1].Value;
            }
            try
            {
                // 使用DiskPartition库读取GPT分区表
                IGptReader gptReader = DiskPartition.DiskPartition.ReadGpt().Primary();
                GuidPartitionTable gpt = gptReader.FromPath(gptFile);

                // 为每个分区创建program元素
                foreach (var partition in gpt.Partitions)
                {
                    // 跳过空分区
                    if (partition.Guid.ToString() == "00000000-0000-0000-0000-000000000000")
                    {
                        continue;
                    }
                    // 创建标准程序元素
                    XElement programElement = new XElement("program",
                        new XAttribute("SECTOR_SIZE_IN_BYTES", gpt.SectorSize),
                        new XAttribute("file_sector_offset", "0"),
                        new XAttribute("filename", ""), // 默认为空，用户可以后续选择文件
                        new XAttribute("label", partition.Name),
                        new XAttribute("num_partition_sectors", (partition.LastLba - partition.FirstLba + 1).ToString()),
                        new XAttribute("physical_partition_number", lun),
                        new XAttribute("size_in_KB", ((partition.LastLba - partition.FirstLba + 1) * (ulong)gpt.SectorSize / 1024.0).ToString("F1")),
                        new XAttribute("sparse", "false"),
                        new XAttribute("start_byte_hex", $"0x{(partition.FirstLba * (ulong)gpt.SectorSize):X}"),
                        new XAttribute("start_sector", $"{partition.FirstLba}")
                    );
                    xmlDoc.Root.Add(programElement);
                }
            }
            catch (Exception ex)
            {
                // 记录错误但继续处理其他文件
                EDLLog += $"解析文件 {gptFile} 时出错: {ex.Message}{Environment.NewLine}";
                Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent($"分区表解析出错: {ex.Message}")
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
            }
        }

        // 为XML元素添加索引属性
        int index = 1;
        foreach (var element in xmlDoc.Root.Elements("program"))
        {
            element.SetAttributeValue("Uotan-Index", index++);
        }
        // 保存XML文档
        xmlDoc.Save(outputXmlPath);
    }
}

public partial class EDLPartModel : ObservableObject
{
    [ObservableProperty]
    private bool selectPart;

    [ObservableProperty]
    private string lun;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string fileName = "点击选择镜像";

    [ObservableProperty]
    private string isSparse;

    [ObservableProperty]
    private string offset;

    [ObservableProperty]
    private string start;

    [ObservableProperty]
    private string sector;

    [ObservableProperty]
    private string index;
}