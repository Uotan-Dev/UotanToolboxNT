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
using System.Text;
using System.Threading.Tasks;
using UotanToolbox.Common;


namespace UotanToolbox.Features.EDL;

public partial class EDLViewModel : MainPageBase
{
    [ObservableProperty]
    private string firehoseFile, currentDevice = "当前连接：COM5", memoryTepy = "存储类型：UFS", xMLFile, partNamr, eDLLog;
    [ObservableProperty]
    private bool auto = true, uFS = false, eMMC = false;
    [ObservableProperty]
    private int selectFlie = 0, selectUFSLun = 0, selectBand = 1;
    [ObservableProperty]
    private AvaloniaList<string> builtInFile = ["common", "mi_auth", "mi_noauth_625", "mi_noauth_778g"], uFSLun = ["6", "7", "8"], bandList = ["commonl", "mi", "oppo", "oneplus", "meizu", "zte", "lg"];
    [ObservableProperty]
    private PartModel partModel;

    private const int LogLevel = 0;
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }
    private async Task qsahara(string port = null,int? verbose = null,string command = null,bool memdump = false,bool image = false,string sahara = null,string prefix = null,string where = null,string ramdumpimage = null,bool efssyncloop = false,int? rxtimeout = null,int? maxwrite = null,string addsearchpath = null,bool sendclearstate = false,int? portnumber = null,bool switchimagetx = false,bool nomodereset = false,string cmdrespfilepath = null,bool uart = false,bool ethernet = false,int? ethernet_port = null,bool noreset = false,bool resettxfail = false)
    {
        await Task.Run(() =>
        {
            List<string> args = [];if (port != null) args.Add($"--port {port}");if (verbose.HasValue) args.Add($"--verbose {verbose}");if (command != null) args.Add($"--command \"{command}\"");if (memdump) args.Add("--memdump");if (image) args.Add("--image  ");if (sahara != null) args.Add($"--sahara \"{sahara}\"");if (prefix != null) args.Add($"--prefix \"{prefix}\"");if (where != null) args.Add($"--where \"{where}\"");if (ramdumpimage != null) args.Add($"--ramdumpimage \"{ramdumpimage}\"");if (efssyncloop) args.Add("--efssyncloop");if (rxtimeout.HasValue) args.Add($"--rxtimeout \"{rxtimeout}\"");if (maxwrite.HasValue) args.Add($"--maxwrite \"{maxwrite}\"");if (addsearchpath != null) args.Add($"--addsearchpath \"{addsearchpath}\"");if (sendclearstate) args.Add("--sendclearstate");if (portnumber.HasValue) args.Add($"--portnumber \"{portnumber}\"");if (switchimagetx) args.Add("--switchimagetx");if (nomodereset) args.Add("--nomodereset");if (cmdrespfilepath != null) args.Add($"--cmdrespfilepath \"{cmdrespfilepath}\"");if (uart) args.Add("--UART");if (ethernet) args.Add("--ethernet");if (ethernet_port.HasValue) args.Add($"--ethernet_port \"{ethernet_port}\"");if (noreset) args.Add("--noreset");if (resettxfail) args.Add("--resettxfail");
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
    private async Task fh_loader(bool benchmarkdigestperformance = false,bool benchmarkreads = false,bool benchmarkwrites = false,string createvipdigests = null,string chaineddigests = null,string contentsxml = null,bool convertprogram2read = false,string digestsperfilename = null,string erase = null,string files = null,bool firmwarewrite = false,string fixgpt = null,string flattenbuildto = null,string flavor = null,bool forcecontentsxmlpaths = false,string getstorageinfo = null,string json_in = null,string labels = null,string loglevel = null,string lun = null,string mainoutputdir = null,string maxpayloadsizeinbytes = null,string memoryname = null,string notfiles = null,string notlabels = null,bool nop = false,bool noprompt = false,string num_sectors = null,string port = null,string start_sector = null,bool verbose = false,bool verify_programming_getsha = false,bool verify_programming = false,string zlpawarehost = null,bool noautoconfigure = false,bool autoconfig = false
            )
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
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StringBuilder sb = new StringBuilder(EDLLog);
                EDLLog = sb.AppendLine(outLine.Data).ToString();
                //EDLLog.CaretIndex = CustomizedflashLog.Text.Length;
            });
        }
    }
    public EDLViewModel() : base("9008刷机", MaterialIconKind.CableData, -350)
    {
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
            await qsahara(port: "\\\\\\\\.\\\\" + CurrentDevice.Replace("当前连接：", ""), sahara: FirehoseFile);
            
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