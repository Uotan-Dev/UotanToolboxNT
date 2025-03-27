using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiskPartitionInfo.FluentApi;
using DiskPartitionInfo.Gpt;
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
    public string work_path = Path.Join(Global.tmp_path, "EDL-" + StringHelper.RandomString(8));
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public EDLViewModel() : base("9008刷机", MaterialIconKind.CableData, -350)
    {

    }

    private async Task QSahara(string port = null, int? verbose = null, string command = null, bool memdump = false, bool image = false, string sahara = null, string prefix = null, string where = null, string ramdumpimage = null, bool efssyncloop = false, int? rxtimeout = null, int? maxwrite = null, string addsearchpath = null, bool sendclearstate = false, int? portnumber = null, bool switchimagetx = false, bool nomodereset = false, string cmdrespfilepath = null, bool uart = false, bool ethernet = false, int? ethernet_port = null, bool noreset = false, bool resettxfail = false)
    {
        await Task.Run(() =>
        {
            List<string> args = []; if (port != null) args.Add($"-p {port}"); if (verbose.HasValue) args.Add($"-v {verbose}"); if (command != null) args.Add($"-c \"{command}\""); if (memdump) args.Add("-m"); if (image) args.Add("-i  "); if (sahara != null) args.Add($"-s \"{sahara}\""); if (prefix != null) args.Add($"-p \"{prefix}\""); if (where != null) args.Add($"-w \"{where}\""); if (ramdumpimage != null) args.Add($"-r \"{ramdumpimage}\""); if (efssyncloop) args.Add("-l"); if (rxtimeout.HasValue) args.Add($"-t \"{rxtimeout}\""); if (maxwrite.HasValue) args.Add($"-j \"{maxwrite}\""); if (addsearchpath != null) args.Add($"-b \"{addsearchpath}\""); if (sendclearstate) args.Add("-k"); if (portnumber.HasValue) args.Add($"-u \"{portnumber}\""); if (switchimagetx) args.Add("-x"); if (nomodereset) args.Add("-o"); if (cmdrespfilepath != null) args.Add($"-a \"{cmdrespfilepath}\""); if (uart) args.Add("-U"); if (ethernet) args.Add("-e"); if (ethernet_port.HasValue) args.Add($"-n \"{ethernet_port}\""); if (noreset) args.Add("--noreset"); if (resettxfail) args.Add("--resettxfail");
            string cmd = Path.Combine(Global.bin_path, "QSaharaServer");
            ProcessStartInfo QSaharaServer = new ProcessStartInfo(cmd, string.Join(" ", args))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process qss = new Process();
            qss.StartInfo = QSaharaServer;
            _ = qss.Start();
            EDLLog = "";
            qss.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            qss.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            qss.BeginOutputReadLine();
            qss.BeginErrorReadLine();
            qss.WaitForExit();
            qss.Close();
        });
    }
    /// <summary>
    /// 同步自动调用FH_Loader，默认不自动配置，日志自动同步到EDLLog
    /// </summary>
    /// <param name="workpath"></param>
    /// <param name="benchmarkdigestperformance"></param>
    /// <param name="benchmarkreads"></param>
    /// <param name="benchmarkwrites"></param>
    /// <param name="createvipdigests"></param>
    /// <param name="chaineddigests"></param>
    /// <param name="contentsxml"></param>
    /// <param name="convertprogram2read"></param>
    /// <param name="digestsperfilename"></param>
    /// <param name="erase"></param>
    /// <param name="files"></param>
    /// <param name="firmwarewrite"></param>
    /// <param name="fixgpt"></param>
    /// <param name="flattenbuildto"></param>
    /// <param name="flavor"></param>
    /// <param name="forcecontentsxmlpaths"></param>
    /// <param name="getstorageinfo"></param>
    /// <param name="json_in"></param>
    /// <param name="labels"></param>
    /// <param name="loglevel"></param>
    /// <param name="lun"></param>
    /// <param name="mainoutputdir"></param>
    /// <param name="maxpayloadsizeinbytes"></param>
    /// <param name="memoryname"></param>
    /// <param name="notfiles"></param>
    /// <param name="notlabels"></param>
    /// <param name="nop"></param>
    /// <param name="noprompt"></param>
    /// <param name="num_sectors"></param>
    /// <param name="port"></param>
    /// <param name="start_sector"></param>
    /// <param name="verbose"></param>
    /// <param name="verify_programming_getsha"></param>
    /// <param name="verify_programming"></param>
    /// <param name="zlpawarehost"></param>
    /// <param name="noautoconfigure"></param>
    /// <param name="autoconfig"></param>
    /// <returns></returns>
    private async Task Fh_loader(string workpath, bool benchmarkdigestperformance = false, bool benchmarkreads = false, bool benchmarkwrites = false, string createvipdigests = null, string chaineddigests = null, string contentsxml = null, bool convertprogram2read = false, string digestsperfilename = null, string erase = null, string files = null, bool firmwarewrite = false, string fixgpt = null, string flattenbuildto = null, string flavor = null, bool forcecontentsxmlpaths = false, string getstorageinfo = null, string json_in = null, string labels = null, string loglevel = null, string lun = null, string mainoutputdir = null, string maxpayloadsizeinbytes = null, string memoryname = null, string notfiles = null, string notlabels = null, bool nop = false, bool noprompt = false, string num_sectors = null, string port = null, string porttracename = null, string port_type = null, string power = null, string search_path = null, string sectorsizeinbytes = null, string sendimage = null, string sendxml = null, string setactivepartition = null, bool showpercentagecomplete = false, string signeddigests = null, bool skip_config = false, string slot = null, string start_sector = null, bool verbose = false, bool verify_programming_getsha = false, bool verify_programming = false, string zlpawarehost = null)
    {
        await Task.Run(() =>
        {
            List<string> args = [];
            string cmd = Path.Combine(Global.bin_path, "fh_loader");
            if (search_path != null) args.Add($"--search_path=\"{search_path}\\\"");
            if (benchmarkdigestperformance) args.Add("--benchmarkdigestperformance");
            if (benchmarkreads) args.Add("--benchmarkreads");
            if (benchmarkwrites) args.Add("--benchmarkwrites");
            if (createvipdigests != null) args.Add($"--createvipdigests=\"{createvipdigests}\"");
            if (chaineddigests != null) args.Add($"--chaineddigests=\"{chaineddigests}\"");
            if (contentsxml != null) args.Add($"--contentsxml=\"{contentsxml}\"");
            if (convertprogram2read) args.Add("--convertprogram2read");
            if (digestsperfilename != null) args.Add($"--digestsperfilename=\"{digestsperfilename}\"");
            if (erase != null) args.Add($"--erase=\"{erase}\"");
            if (files != null) args.Add($"--files=\"{files}\"");
            if (firmwarewrite) args.Add("--firmwarewrite");
            if (fixgpt != null) args.Add($"--fixgpt=\"{fixgpt}\"");
            if (flattenbuildto != null) args.Add($"--flattenbuildto=\"{flattenbuildto}\"");
            if (flavor != null) args.Add($"--flavor=\"{flavor}\"");
            if (forcecontentsxmlpaths) args.Add("--forcecontentsxmlpaths");
            if (getstorageinfo != null) args.Add($"--getstorageinfo=\"{getstorageinfo}\"");
            if (json_in != null) args.Add($"--json_in=\"{json_in}\"");
            if (labels != null) args.Add($"--labels=\"{labels}\"");
            if (loglevel != null) args.Add($"--loglevel=\"{loglevel}\"");
            if (lun != null) args.Add($"--lun=\"{lun}\"");
            if (mainoutputdir != null) args.Add($"--mainoutputdir=\"{mainoutputdir}\"");
            if (maxpayloadsizeinbytes != null) args.Add($"--maxpayloadsizeinbytes=\"{maxpayloadsizeinbytes}\"");
            if (memoryname != null) args.Add($"--memoryname=\"{memoryname}\"");
            if (notfiles != null) args.Add($"--notfiles=\"{notfiles}\"");
            if (notlabels != null) args.Add($"--notlabels=\"{notlabels}\"");
            if (nop) args.Add("--nop");
            if (noprompt) args.Add("--noprompt");
            if (num_sectors != null) args.Add($"--num_sectors=\"{num_sectors}\"");
            if (port != null) args.Add($"--port={port}");
            if (porttracename != null) args.Add($"--porttracename=\"{porttracename}\"");
            if (port_type != null) args.Add($"--port_type=\"{port_type}\"");
            if (power != null) args.Add($"--power=\"{power}\"");
            if (sectorsizeinbytes != null) args.Add($"--sectorsizeinbytes=\"{sectorsizeinbytes}\"");
            if (sendimage != null) args.Add($"--sendimage=\"{sendimage}\"");
            if (sendxml != null) args.Add($"--sendxml=\"{sendxml}\"");
            if (setactivepartition != null) args.Add($"--setactivepartition=\"{setactivepartition}\"");
            if (showpercentagecomplete) args.Add("--showpercentagecomplete");
            if (signeddigests != null) args.Add($"--signeddigests=\"{signeddigests}\"");
            if (slot != null) args.Add($"--slot=\"{slot}\"");
            if (skip_config) args.Add("--skip_config");
            if (start_sector != null) args.Add($"--start_sector=\"{start_sector}\"");
            if (verbose) args.Add("--verbose");
            if (verify_programming_getsha) args.Add("--verify_programming_getsha");
            if (verify_programming) args.Add("--verify_programming");
            if (zlpawarehost != null) args.Add($"--zlpawarehost=\"{zlpawarehost}\"");
            Directory.SetCurrentDirectory(workpath);
            ProcessStartInfo QSaharaServer = new ProcessStartInfo(cmd, string.Join(" ", args))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            using Process qss = new Process();
            qss.StartInfo = QSaharaServer;
            EDLLog = "";
            _ = qss.Start();
            qss.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            qss.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            qss.BeginOutputReadLine();
            qss.BeginErrorReadLine();
            qss.WaitForExit();
            qss.Close();
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
            var rawprogramFiles = Directory.GetFiles(folderPath, "rawprogram*.xml");
            var patchFiles = Directory.GetFiles(folderPath, "patch*.xml");
            var firehoseFiles = Directory.GetFiles(folderPath, "*elf");
            var rawprogramXmls = rawprogramFiles.Select(file => new FileInfo(file)).ToList();
            var patchXmls = patchFiles.Select(file => new FileInfo(file)).ToList();
            var xmlFiles = new List<string>();
            Directory.CreateDirectory(work_path);
            xmlFiles.AddRange(rawprogramXmls.Select(file => file.FullName));
            xmlFiles.AddRange(patchXmls.Select(file => file.FullName));
            Global.xml_path = Path.Join(work_path, "Merged.xml");
            MergeXMLFiles(xmlFiles, Global.xml_path);
            AddIndexToProgramElements(Global.xml_path);
            UpdateProgramElementsWithAbsolutePaths(Global.xml_path, PartNamr);
            DiskTypePrase(Global.xml_path);
            EDLPartModel = [.. ParseProgramElements(Global.xml_path)];
            XMLFile = string.Join(", ", xmlFiles);
            patch_xml_paths = string.Join(", ", patchXmls.Select(file => file.FullName));
            if (firehoseFiles.Length > 0)
            {
                FirehoseFile = firehoseFiles[0];
            }
        }
    }

    private void DiskTypePrase(string xml_path)
    {
        try
        {
            XDocument xdoc = XDocument.Load(xml_path);
            var sectorSizeElement = xdoc.Descendants("program")
                                        .FirstOrDefault()
                                        ?.Attribute("SECTOR_SIZE_IN_BYTES");

            if (sectorSizeElement != null)
            {
                int sectorSize = int.Parse(sectorSizeElement.Value);
                if (sectorSize == 4096)
                {
                    MemoryType = "存储类型：UFS";
                }
                else if (sectorSize == 512)
                {
                    MemoryType = "存储类型：EMMC";
                }
            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                                    .WithTitle(GetTranslation("Common_Warn"))
                                    .OfType(NotificationType.Warning)
                                    .WithContent("该xml文件疑似不可用")
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
            }
        }
        catch (Exception ex)
        {
            Global.MainDialogManager.CreateDialog()
                                    .WithTitle(GetTranslation("Common_Warn"))
                                    .OfType(NotificationType.Warning)
                                    .WithContent(ex.Message)
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
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
        var xmlFiles = new List<string>();
        if (rawprogram_xmls != null)
        {
            xmlFiles.AddRange(rawprogram_xmls.Select(file => file.Path.LocalPath));
        }
        Global.xml_path = Path.Join(work_path, "Merged.xml");
        MergeXMLFiles(xmlFiles, Global.xml_path);
        AddIndexToProgramElements(Global.xml_path);
        UpdateProgramElementsWithAbsolutePaths(Global.xml_path, Path.GetDirectoryName(xmlFiles[0]));
        EDLPartModel = [.. ParseProgramElements(Global.xml_path)];
        XMLFile = string.Join(", ", xmlFiles);
        if (patch_xmls != null)
        {
            xmlFiles.AddRange(patch_xmls.Select(file => file.Path.LocalPath));
        }
        patch_xml_paths = string.Join(", ", xmlFiles);
    }

    [RelayCommand]
    public async Task SendFirehose()
    {
        try
        {
            if (String.IsNullOrEmpty(FirehoseFile))
            {
                throw new Exception("未选择引导文件");
            }
            await QSahara(port: "\\\\.\\" + Global.thisdevice, sahara: "13:" + FirehoseFile);
            FileHelper.Write(edl_log_path, output);
            if (EDLLog.Contains("File transferred successfully"))
            {
                Global.MainDialogManager.CreateDialog()
                            .WithTitle(GetTranslation("Common_Succ"))
                            .OfType(NotificationType.Success)
                            .WithContent("引导发送成功")
                            .Dismiss().ByClickingBackground()
                            .TryShow();
            }
            else if (EDLLog.Contains("Could not connect to"))
            {
                Global.MainDialogManager.CreateDialog()
                            .WithTitle(GetTranslation("Common_Error"))
                            .OfType(NotificationType.Error)
                            .WithContent("引导发送失败")
                            .Dismiss().ByClickingBackground()
                            .TryShow();

            }
            else
            {
                Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent("引导发送失败,原因未知")
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
            }
        }
        catch (Exception ex)
        {
            Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent(ex.Message)
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
        }
    }

    /// <summary>
    /// 读取分区表文件，并生成xml文件然后加载进可视化编辑器
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task ReadPartTable()
    {
        string part_table_path = Path.Join(Global.tmp_path, "Partition_Table");
        string bin_xml_path = Path.Join(Global.bin_path, "XML");
        await DetectDeviceType();
        if (MemoryType == "存储类型：")
        {
            Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Error"))
                        .OfType(NotificationType.Error)
                        .WithContent("未检测到存储类型")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
            return;
        }
        else if (MemoryType == "存储类型：EMMC")
        {
            await Fh_loader(part_table_path, sendxml: Path.Join(bin_xml_path, "ReadPartitionTable.xml"), showpercentagecomplete: true, noprompt: true, port: "\\\\.\\" + Global.thisdevice, search_path: PartNamr, memoryname: MemoryType.Replace("存储类型：", "").Trim().ToLower());
            if (!EDLLog.Contains("All Finished Successfully"))
            {
                Global.MainDialogManager.CreateDialog()
                            .WithTitle(GetTranslation("Common_Error"))
                            .OfType(NotificationType.Error)
                            .WithContent("分区表读取失败")
                            .Dismiss().ByClickingBackground()
                            .TryShow();
                return;
            }
        }
        else if (MemoryType == "存储类型：UFS")
        {
            await Fh_loader(part_table_path, sendxml: Path.Join(bin_xml_path, "ReadPartitionTable6.xml"), showpercentagecomplete: true, noprompt: true, port: "\\\\.\\" + Global.thisdevice, search_path: PartNamr, memoryname: MemoryType.Replace("存储类型：", "").Trim().ToLower());
            if (!EDLLog.Contains("All Finished Successfully"))
            {
                Global.MainDialogManager.CreateDialog()
                            .WithTitle(GetTranslation("Common_Error"))
                            .OfType(NotificationType.Error)
                            .WithContent("分区表读取失败")
                            .Dismiss().ByClickingBackground()
                            .TryShow();
                return;
            }
            await Fh_loader(part_table_path, sendxml: Path.Join(bin_xml_path, "ReadPartitionTable7.xml"), showpercentagecomplete: true, noprompt: true, port: "\\\\.\\" + Global.thisdevice, search_path: PartNamr, memoryname: MemoryType.Replace("存储类型：", "").Trim().ToLower());
            if (EDLLog.Contains("There is a chance your target is in SAHARA mode"))
            {
                Global.MainDialogManager.CreateDialog()
                                        .WithTitle(GetTranslation("Common_Error"))
                                        .OfType(NotificationType.Error)
                                        .WithContent("需要重新进入EDL模式")
                                        .Dismiss().ByClickingBackground()
                                        .TryShow();
                return;
            }
            if (EDLLog.Contains("All Finished Successfully"))
            {
                await Fh_loader(part_table_path, sendxml: Path.Join(bin_xml_path, "ReadPartitionTable8.xml"), showpercentagecomplete: true, noprompt: true, port: "\\\\.\\" + Global.thisdevice, search_path: PartNamr, memoryname: MemoryType.Replace("存储类型：", "").Trim().ToLower());
                if (EDLLog.Contains("There is a chance your target is in SAHARA mode"))
                {
                    Global.MainDialogManager.CreateDialog()
                                            .WithTitle(GetTranslation("Common_Error"))
                                            .OfType(NotificationType.Error)
                                            .WithContent("需要重新进入EDL模式")
                                            .Dismiss().ByClickingBackground()
                                            .TryShow();
                    return;
                }
            }
        }
        Global.xml_path = Path.Join(work_path, "Merged.xml");
        ReadGptFilesAndGenerateXml(part_table_path, Global.xml_path);
        AddIndexToProgramElements(Global.xml_path);
        EDLPartModel = [.. ParseProgramElements(Global.xml_path)];
    }

    [RelayCommand]
    public async Task WritePart()
    {
        await DetectDeviceType();
        if (MemoryType == "存储类型：")
        {
            Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Error"))
                        .OfType(NotificationType.Error)
                        .WithContent("未检测到存储类型")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
            return;
        }
        string tmp_xml_path = Path.Join(work_path, "Temp.xml");
        ExtractSelectedPartsToXml(tmp_xml_path);
        RemoveIndexToProgramElements(tmp_xml_path);
        await Fh_loader(Global.bin_path, sendxml: tmp_xml_path, showpercentagecomplete: true, noprompt: true, port: "\\\\.\\" + Global.thisdevice, memoryname: MemoryType.Replace("存储类型：", "").Trim().ToLower());
        if (EDLLog.Contains("There is a chance your target is in SAHARA mode"))
        {
            Global.MainDialogManager.CreateDialog()
                                    .WithTitle(GetTranslation("Common_Error"))
                                    .OfType(NotificationType.Error)
                                    .WithContent("需要重新进入EDL模式")
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
            return;
        }
        FileHelper.Write(edl_log_path, output);
        if (EDLLog.Contains("All Finished Successfully"))
        {
            Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Succ"))
                        .OfType(NotificationType.Success)
                        .WithContent("文件发送成功")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
        }
        if (EDLLog.Contains("ERROR: Please see log"))
        {
            Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Error"))
                        .OfType(NotificationType.Error)
                        .WithContent("文件发送失败,详见日志")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
        }
    }

    [RelayCommand]
    public async Task ReadPart()
    {
        await DetectDeviceType();
        if (MemoryType == "存储类型：")
        {
            Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Error"))
                        .OfType(NotificationType.Error)
                        .WithContent("未检测到存储类型")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
            return;
        }
        string tmp_xml_path = Path.Join(work_path, "Temp.xml");
        ExtractSelectedPartsToXml(tmp_xml_path);
        RemoveIndexToProgramElements(tmp_xml_path);
        RenameProgramNodesToErase(tmp_xml_path);
        await Fh_loader(Global.bin_path, sendxml: tmp_xml_path, convertprogram2read: true, showpercentagecomplete: true, noprompt: true, port: "\\\\.\\" + Global.thisdevice, memoryname: MemoryType.Replace("存储类型：", "").Trim().ToLower());
        FileHelper.Write(edl_log_path, output);
        if (EDLLog.Contains("There is a chance your target is in SAHARA mode"))
        {
            Global.MainDialogManager.CreateDialog()
                                    .WithTitle(GetTranslation("Common_Error"))
                                    .OfType(NotificationType.Error)
                                    .WithContent("需要重新进入EDL模式")
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
            return;
        }
        if (EDLLog.Contains("All Finished Successfully"))
        {
            Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Succ"))
                        .OfType(NotificationType.Success)
                        .WithContent("分区读取成功")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
        }
        if (EDLLog.Contains("ERROR: Please see log"))
        {
            Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Error"))
                        .OfType(NotificationType.Error)
                        .WithContent("文件读取失败,详见日志")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
        }
    }

    [RelayCommand]
    public async Task ErasePart()
    {
        await DetectDeviceType();
        if (MemoryType == "存储类型：")
        {
            Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Error"))
                        .OfType(NotificationType.Error)
                        .WithContent("未检测到存储类型")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
            return;
        }
        string tmp_xml_path = Path.Join(Global.tmp_path, StringHelper.RandomString(16) + ".xml");
        ExtractSelectedPartsToXml(tmp_xml_path);
        RemoveIndexToProgramElements(tmp_xml_path);
        RenameProgramNodesToErase(tmp_xml_path);
        await Fh_loader(Global.bin_path, sendxml: tmp_xml_path, convertprogram2read: true, showpercentagecomplete: true, noprompt: true, port: "\\\\.\\" + Global.thisdevice, search_path: PartNamr, memoryname: MemoryType.Replace("存储类型：", "").Trim().ToLower());
        FileHelper.Write(edl_log_path, output);
        if (EDLLog.Contains("There is a chance your target is in SAHARA mode"))
        {
            Global.MainDialogManager.CreateDialog()
                                    .WithTitle(GetTranslation("Common_Error"))
                                    .OfType(NotificationType.Error)
                                    .WithContent("需要重新进入EDL模式")
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
            return;
        }
        if (EDLLog.Contains("All Finished Successfully"))
        {
            Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Succ"))
                        .OfType(NotificationType.Success)
                        .WithContent("分区读取成功")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
        }
        if (EDLLog.Contains("ERROR: Please see log"))
        {
            Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Error"))
                        .OfType(NotificationType.Error)
                        .WithContent("文件读取失败,详见日志")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
        }
    }

    [RelayCommand]
    public async Task RebootEDL()
    {
        await Fh_loader(Global.bin_path, sendxml: Path.Join(Global.bin_path, "XML", "EDL.xml"), showpercentagecomplete: true, noprompt: true, port: "\\\\.\\" + Global.thisdevice);
        FileHelper.Write(edl_log_path, output);
        if (EDLLog.Contains("All Finished Successfully"))
        {
            Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Succ"))
                        .OfType(NotificationType.Success)
                        .WithContent("重启文件发送成功")
                        .Dismiss().ByClickingBackground()
                        .TryShow();
        }
        if (EDLLog.Contains("There is a chance your target is in SAHARA mode"))
        {
            Global.MainDialogManager.CreateDialog()
                                    .WithTitle(GetTranslation("Common_Error"))
                                    .OfType(NotificationType.Error)
                                    .WithContent("需要重新进入EDL模式")
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
            return;
        }
    }

    [RelayCommand]
    public async Task RebootSys()
    {
        await Fh_loader(Global.bin_path, sendxml: Path.Join(Global.bin_path, "XML", "Continue.xml"), showpercentagecomplete: true, noprompt: true, port: "\\\\.\\" + Global.thisdevice);
        FileHelper.Write(edl_log_path, output);
        if (EDLLog.Contains("There is a chance your target is in SAHARA mode"))
        {
            Global.MainDialogManager.CreateDialog()
                                    .WithTitle(GetTranslation("Common_Error"))
                                    .OfType(NotificationType.Error)
                                    .WithContent("需要重新进入EDL模式")
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
            return;
        }
        if (EDLLog.Contains("All Finished Successfully"))
        {
            Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Succ"))
                        .OfType(NotificationType.Success)
                        .WithContent("重启文件发送成功")
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
    /// 合并用户选择的XML文件
    /// </summary>
    /// <param name="xmlFilePaths">多个xml文件所在目录</param>
    /// <param name="outputFilePath">输出的xml文件路径，具体到文件名</param>
    private void MergeXMLFiles(IEnumerable<string> xmlFilePaths, string outputFilePath)
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
    /// 解析GPT分区表文件，生成XML文件，节点为标准xml，不包含厂商定义节点
    /// </summary>
    /// <param name="directoryPath">分区表文件所在文件夹</param>
    /// <param name="outputXmlPath">输出的xml文件路径，具体到文件名</param>
    private void ReadGptFilesAndGenerateXml(string directoryPath, string outputXmlPath)
    {
        if (MemoryType == "存储类型：EMMC")
        {
            Global.SectorSize = 512;
        }
        else if (MemoryType == "存储类型：UFS")
        {
            Global.SectorSize = 4096;
        }
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
            IGptReader gptReader = DiskPartitionInfo.DiskPartitionInfo.ReadGpt().Primary();
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
        XDocument tempDoc = XDocument.Load(Global.xml_path);
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
    /// <summary>
    /// 检测设备存储类型，若为自动，则尝试检测 UFS 和 EMMC。由于市面UFS设备更多，故先检测 UFS
    /// </summary>
    /// <returns></returns>
    private async Task DetectDeviceType()
    {

        try
        {
            string tmpBinFilePath = Path.Combine(Global.tmp_path, "tmp.bin");
            // 尝试检测 UFS 存储类型
            bool isUfs = await TryDetectMemoryType("ufs", "UFS.xml", tmpBinFilePath);
            if (isUfs)
            {
                MemoryType = "存储类型：UFS";
                return;
            }
            // 尝试检测 EMMC 存储类型
            bool isEmmc = await TryDetectMemoryType("emmc", "EMMC.xml", tmpBinFilePath);
            if (isEmmc)
            {
                MemoryType = "存储类型：EMMC";
                return;
            }

            // 如果都未检测到，设置为空
            MemoryType = "存储类型：";
        }
        catch (Exception ex)
        {
            Global.MainDialogManager.CreateDialog()
                                    .WithTitle(GetTranslation("Common_Error"))
                                    .OfType(NotificationType.Error)
                                    .WithContent(ex.Message)
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
            MemoryType = "存储类型：";
        }
    }

    private async Task<bool> TryDetectMemoryType(string memoryName, string xmlFileName, string tmpBinFilePath)
    {
        await Fh_loader(Global.tmp_path, memoryname: memoryName, sendxml: Path.Join(Global.bin_path, "XML", xmlFileName), convertprogram2read: true, showpercentagecomplete: true, noprompt: true, port: "\\\\.\\" + Global.thisdevice);
        FileHelper.Write(edl_log_path, output);
        int sector = 0;
        if (memoryName == "ufs")
        {
            sector = 4096;
        }
        else if (memoryName == "emmc")
        {
            sector = 512;
        }
        if (EDLLog.Contains("All Finished Successfully") && File.Exists(tmpBinFilePath))
        {
            FileInfo fileInfo = new FileInfo(tmpBinFilePath);
            return fileInfo.Length == sector;
        }
        if (EDLLog.Contains("There is a chance your target is in SAHARA mode"))
        {
            Global.MainDialogManager.CreateDialog()
                                    .WithTitle(GetTranslation("Common_Error"))
                                    .OfType(NotificationType.Error)
                                    .WithContent("需要重新进入EDL模式")
                                    .Dismiss().ByClickingBackground()
                                    .TryShow();
            return false;
        }
        return false;
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
