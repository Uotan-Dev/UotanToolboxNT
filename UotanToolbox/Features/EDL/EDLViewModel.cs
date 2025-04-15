using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiskPartitionInfo;
using DiskPartitionInfo.FluentApi;
using DiskPartitionInfo.Gpt;
using Material.Icons;
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
    private AvaloniaList<string> bandList = ["通用", "小米", "OPPO", "一加", "魅族", "中兴", "LG"], seriesList = ["选择系列"], modelList = ["选择机型"];
    [ObservableProperty]
    private AvaloniaList<EDLPartModel> eDLPartModel = [];
    private string patch_xml_paths = "";
    private string output = "";
    private readonly string edl_log_path = Path.Combine(Global.log_path, "edl.txt");
    private const int LogLevel = 0;
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
            XmlPreprocess(rawprogramXmls, patchXmls); // XML文件预处理，主要为工作目录创建，xml文件合并，节点解析并可视化处理
            var xmlFiles = new List<string>();
            xmlFiles.AddRange(rawprogramXmls.Select(file => file.FullName));
            xmlFiles.AddRange(patchXmls.Select(file => file.FullName));
            EDLPartModel = [.. ParseProgramElements(Path.Join(work_path, "Merged.xml"))];
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

    private void XmlPreprocess(List<FileInfo> rawprogramXmls, List<FileInfo> patchXmls)
    {
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
        MergeProgramXMLFiles(rawprogramXmls.Select(file => file.FullName), Path.Join(work_path, "Merged.xml"));
        MergePatchXMLFiles(rawprogramXmls.Select(file => file.FullName), Path.Join(work_path, "Merged_Patch.xml"));
        //为合并后的xml文件添加索引号
        AddIndexToProgramElements(Path.Join(work_path, "Merged.xml"));
        //将绝对地址添加到xml文件中
        UpdateProgramElementsWithAbsolutePaths(Path.Join(work_path, "Merged.xml"), PartNamr);
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
                FileName = Path.GetFileName(element.Attribute("filename")?.Value),//让文件名只显示文件名，不显示路径
                IsSparse = element.Attribute("sparse")?.Value,
                Offset = element.Attribute("file_sector_offset")?.Value,
                Start = element.Attribute("start_sector")?.Value,
                Sector = element.Attribute("num_partition_sectors")?.Value,
                Index = element.Attribute("Uotan-Index")?.Value
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
    }

    /// <summary>
    /// 合并用户选择的patch.xml文件
    /// </summary>
    /// <param name="xmlFilePaths">多个xml文件所在目录</param>
    /// <param name="outputFilePath">输出的xml文件路径，具体到文件名</param>
    private void MergePatchXMLFiles(IEnumerable<string> xmlFilePaths, string outputFilePath)
    {
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
    }

    /// <summary>
    /// 解析GPT分区表文件，生成XML文件，节点为标准xml，不包含厂商定义节点
    /// </summary>
    /// <param name="directoryPath">分区表文件所在文件夹</param>
    /// <param name="outputXmlPath">输出的xml文件路径，具体到文件名</param>
    private void ReadGptFilesAndGenerateXml(string directoryPath, string outputXmlPath)
    {
        XDocument xmlDoc = new XDocument(new XElement("data"));
        string pattern = @"gpt_main(\d+)\.bin";
        // 获取目录下所有gpt_main*.bin文件
        var gptFiles = Directory.GetFiles(directoryPath, "gpt_main*.bin");

        foreach (var gptFile in gptFiles)
        {
            Match match = Regex.Match(Path.GetFileName(gptFile), pattern);
            string number = "";
            if (match.Success)
            {
                number = match.Groups[1].Value;
            }
            IGptReader gptReader = DiskPartition.ReadGpt().Primary();
            GuidPartitionTable gpt = gptReader.FromPath(gptFile);
            XElement root = new("data");
            foreach (var partition in gpt.Partitions)
            {
                if (partition.Guid.ToString() == "00000000-0000-0000-0000-000000000000")
                {
                    continue;
                }
                XElement partitionElement = new XElement("Partition",
                                            new XAttribute("SECTOR_SIZE_IN_BYTES", Global.SectorSize),
                                            new XAttribute("file_sector_offset", "0"),
                                            new XAttribute("filename", ""),
                                            new XAttribute("label", partition.Name),
                                            new XAttribute("num_partition_sectors", (partition.LastLba - partition.FirstLba + 1).ToString()),
                                            new XAttribute("physical_partition_number", number),
                                            new XAttribute("size_in_KB", ((partition.LastLba - partition.FirstLba + 1) * (ulong)Global.SectorSize / 1024.0).ToString("F1")),
                                            new XAttribute("sparse", "false"),
                                            new XAttribute("start_byte_hex", $"{partition.FirstLba * (ulong)Global.SectorSize}"),
                                            new XAttribute("start_sector", $"{partition.FirstLba}")
                                            );
                root.Add(partitionElement);
            }
            XDocument doc = new(new XDeclaration("1.0", "utf-8", "yes"), root);
            doc.Save("Partitions.xml");
        }
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
