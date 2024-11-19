using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UotanToolbox.Common;


namespace UotanToolbox.Features.EDL;

public partial class EDLViewModel : MainPageBase
{
    [ObservableProperty]
    private string firehoseFile, currentDevice = "当前连接：COM6", memoryTepy = "存储类型：UFS", xMLFile, partNamr, eDLLog;
    [ObservableProperty]
    private bool auto = true, uFS = false, eMMC = false;
    [ObservableProperty]
    private int selectFlie = 0, selectUFSLun = 0, selectBand = 1, logIndex = 0;
    [ObservableProperty]
    private AvaloniaList<string> builtInFile = ["common", "mi_auth", "mi_noauth_625", "mi_noauth_778g"], uFSLun = ["6", "7", "8"], bandList = ["commonl", "mi", "oppo", "oneplus", "meizu", "zte", "lg"];
    [ObservableProperty]
    private AvaloniaList<EDLPartModel> eDLPartModel = [];

    private string output = "";
    private readonly string edl_log_path = Path.Combine(Global.log_path, "edl.txt");
    private const int LogLevel = 0;
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public EDLViewModel() : base("9008刷机", MaterialIconKind.CableData, -350)
    {
        eDLPartModel.AddRange(Enumerable.Range(1, 50).Select(x => new EDLPartModel()));
    }

    private async Task QSahara(string port = null,int? verbose = null,string command = null,bool memdump = false,bool image = false,string sahara = null,string prefix = null,string where = null,string ramdumpimage = null,bool efssyncloop = false,int? rxtimeout = null,int? maxwrite = null,string addsearchpath = null,bool sendclearstate = false,int? portnumber = null,bool switchimagetx = false,bool nomodereset = false,string cmdrespfilepath = null,bool uart = false,bool ethernet = false,int? ethernet_port = null,bool noreset = false,bool resettxfail = false)
    {
        await Task.Run( () =>
        {
            List<string> args = [];if (port != null) args.Add($"-p {port}");if (verbose.HasValue) args.Add($"-v {verbose}");if (command != null) args.Add($"-c \"{command}\"");if (memdump) args.Add("-m");if (image) args.Add("-i  ");if (sahara != null) args.Add($"-s \"{sahara}\"");if (prefix != null) args.Add($"-p \"{prefix}\"");if (where != null) args.Add($"-w \"{where}\"");if (ramdumpimage != null) args.Add($"-r \"{ramdumpimage}\"");if (efssyncloop) args.Add("-l");if (rxtimeout.HasValue) args.Add($"-t \"{rxtimeout}\"");if (maxwrite.HasValue) args.Add($"-j \"{maxwrite}\"");if (addsearchpath != null) args.Add($"-b \"{addsearchpath}\"");if (sendclearstate) args.Add("-k");if (portnumber.HasValue) args.Add($"-u \"{portnumber}\"");if (switchimagetx) args.Add("-x");if (nomodereset) args.Add("-o");if (cmdrespfilepath != null) args.Add($"-a \"{cmdrespfilepath}\"");if (uart) args.Add("-U");if (ethernet) args.Add("-e");if (ethernet_port.HasValue) args.Add($"-n \"{ethernet_port}\"");if (noreset) args.Add("--noreset");if (resettxfail) args.Add("--resettxfail");
            string cmd = Path.Combine(Global.bin_path, "QSaharaServer");
            ProcessStartInfo QSaharaServer = new ProcessStartInfo(cmd, string.Join(" ", args))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            using Process qss = new Process();
            qss.StartInfo = QSaharaServer;
            _ = qss.Start();
            qss.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            qss.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            qss.BeginOutputReadLine();
            qss.BeginErrorReadLine();
            qss.WaitForExit();
            qss.Close();
        });
    }

    private async Task Fh_loader(bool benchmarkdigestperformance = false,bool benchmarkreads = false,bool benchmarkwrites = false,string createvipdigests = null,string chaineddigests = null,string contentsxml = null,bool convertprogram2read = false,string digestsperfilename = null,string erase = null,string files = null,bool firmwarewrite = false,string fixgpt = null,string flattenbuildto = null,string flavor = null,bool forcecontentsxmlpaths = false,string getstorageinfo = null,string json_in = null,string labels = null,string loglevel = null,string lun = null,string mainoutputdir = null,string maxpayloadsizeinbytes = null,string memoryname = null,string notfiles = null,string notlabels = null,bool nop = false,bool noprompt = false,string num_sectors = null,string port = null,string start_sector = null,bool verbose = false,bool verify_programming_getsha = false,bool verify_programming = false,string zlpawarehost = null,bool noautoconfigure = false,bool autoconfig = false)
    {
        await Task.Run(() =>
        {
            List<string> args = [];
            string cmd = Path.Combine(Global.bin_path, "fh_loader");if (benchmarkdigestperformance) args.Add("--benchmarkdigestperformance");if (benchmarkreads) args.Add("--benchmarkreads");if (benchmarkwrites) args.Add("--benchmarkwrites");if (createvipdigests != null) args.Add($"--createvipdigests=\"{createvipdigests}\"");if (chaineddigests != null) args.Add($"--chaineddigests=\"{chaineddigests}\"");if (contentsxml != null) args.Add($"--contentsxml=\"{contentsxml}\"");if (convertprogram2read) args.Add("--convertprogram2read");if (digestsperfilename != null) args.Add($"--digestsperfilename=\"{digestsperfilename}\"");if (erase != null) args.Add($"--erase=\"{erase}\"");if (files != null) args.Add($"--files=\"{files}\"");if (firmwarewrite) args.Add("--firmwarewrite");if (fixgpt != null) args.Add($"--fixgpt=\"{fixgpt}\"");if (flattenbuildto != null) args.Add($"--flattenbuildto=\"{flattenbuildto}\"");if (flavor != null) args.Add($"--flavor=\"{flavor}\"");if (forcecontentsxmlpaths) args.Add("--forcecontentsxmlpaths");if (getstorageinfo != null) args.Add($"--getstorageinfo=\"{getstorageinfo}\"");if (json_in != null) args.Add($"--json_in=\"{json_in}\"");if (labels != null) args.Add($"--labels=\"{labels}\"");if (loglevel != null) args.Add($"--loglevel=\"{loglevel}\"");if (lun != null) args.Add($"--lun=\"{lun}\"");if (mainoutputdir != null) args.Add($"--mainoutputdir=\"{mainoutputdir}\"");if (maxpayloadsizeinbytes != null) args.Add($"--maxpayloadsizeinbytes=\"{maxpayloadsizeinbytes}\"");if (memoryname != null) args.Add($"--memoryname=\"{memoryname}\"");if (notfiles != null) args.Add($"--notfiles=\"{notfiles}\"");if (notlabels != null) args.Add($"--notlabels=\"{notlabels}\"");if (nop) args.Add("--nop");if (noprompt) args.Add("--noprompt");if (num_sectors != null) args.Add($"--num_sectors=\"{num_sectors}\"");if (port != null) args.Add($"--port={port}");if (start_sector != null) args.Add($"--start_sector=\"{start_sector}\"");if (verbose) args.Add("--verbose");if (verify_programming_getsha) args.Add("--verify_programming_getsha");if (verify_programming) args.Add("--verify_programming");if (zlpawarehost != null) args.Add($"--zlpawarehost=\"{zlpawarehost}\"");if (noautoconfigure) args.Add("--noautoconfigure");if (autoconfig) args.Add("--autoconfig");
            ProcessStartInfo QSaharaServer = new ProcessStartInfo(cmd, string.Join(" ", args))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            using Process qss = new Process();
            qss.StartInfo = QSaharaServer;
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

    [RelayCommand]
    public async Task SendFirehose()
    {
        try
        {
            if (String.IsNullOrEmpty(FirehoseFile))
            {
                throw new Exception("未选择Firehose文件");

            }
            await QSahara(port: "\\\\.\\" + CurrentDevice.Replace("当前连接：", ""), sahara: "13:" + FirehoseFile);
            FileHelper.Write(edl_log_path, output);
            if (output.Contains("All is well ** SUCCESS!!"))
            {
                Global.MainDialogManager.CreateDialog()
                            .WithTitle(GetTranslation("Common_Success"))
                            .OfType(NotificationType.Success)
                            .WithContent("引导发送成功")
                            .Dismiss().ByClickingBackground()
                            .TryShow();
            }
            else if (output.Contains("Could not connect to"))
            {
                Global.MainDialogManager.CreateDialog()
                            .WithTitle(GetTranslation("Common_Error"))
                            .OfType(NotificationType.Error)
                            .WithContent("引导发送失败")
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
    private string fileName;

    [ObservableProperty]
    private string isSparse;

    [ObservableProperty]
    private string offset;

    [ObservableProperty]
    private string start;

    [ObservableProperty]
    private string sector;
}
