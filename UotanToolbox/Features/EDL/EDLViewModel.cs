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
using Material.Icons;
using SukiUI.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using UotanToolbox.Common;
using EDLLibrary;


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
    private AvaloniaList<string> bandList = ["通用"], seriesList = ["通用"], modelList = ["通用"];
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
        _flash = Flash.Instance;
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
                .WithTitle(GetTranslation("Common_Error"))
                .OfType(NotificationType.Error)
                .WithContent("引导发送成功")
                .Dismiss().ByClickingBackground()
                .TryShow();
            EDLLog += $"引导发送成功{Environment.NewLine}";
        }
        else if (result == "needsig")
        {
            EDLLog += $"需要签名{Environment.NewLine}";

        }
    }

    [RelayCommand]
    public async Task Remark()
    {
        // 检查是否所有项的 SelectPart 都为 true
        bool allSelected = EDLPartModel.All(part => part.SelectPart);

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
            _flash = Flash.Instance;

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

    }

    [RelayCommand]
    public async Task ReadPart()
    {

    }

    [RelayCommand]
    public async Task ErasePart()
    {

    }

    [RelayCommand]
    public async Task RebootEDL()
    {

    }

    [RelayCommand]
    public async Task RebootSys()
    {

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
    /// <summary>
    /// 将图形化界面中选中的部分提取到新的XML文件中，按照Uotan-Index节点进行筛选
    /// </summary>
    /// <param name="outputFilePath">提取后的XML文件输出路径</param>
    private void ExtractSelectedPartsToXml(string outputFilePath)
    {
        XDocument extractedDoc = new XDocument(new XElement("data"));
        XDocument tempDoc = XDocument.Load(Path.Join(work_path, "Merged.xml"));
        foreach (var part in EDLPartModel)
        {
            if (part.SelectPart)
            {
                var element = tempDoc.Descendants("program")
                                     .FirstOrDefault(e => e.Attribute("Uotan-Index")?.Value == part.Index);
                if (element != null)
                {
                    extractedDoc.Root.Add(new XElement(element));
                }
            }
        }
        extractedDoc.Save(outputFilePath);
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
