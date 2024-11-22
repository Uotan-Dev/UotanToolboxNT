using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UotanToolbox.Common;

namespace UotanToolbox.Features.EDL;

public partial class EDLView : UserControl
{
    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public EDLView()
    {
        InitializeComponent();
    }

    public async Task QSaharaServerAsync(string fb)//QSaharaServer实时输出
    {
        await Task.Run(() =>
        {
            string cmd = Path.Combine(Global.bin_path, "edl", "QSaharaServer");
            ProcessStartInfo fastboot = null;
            fastboot = new ProcessStartInfo(cmd, fb);
            fastboot.CreateNoWindow = true;
            fastboot.UseShellExecute = false;
            fastboot.RedirectStandardOutput = true;
            fastboot.RedirectStandardError = true;
            Process f = Process.Start(fastboot);
            f.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            f.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            f.BeginOutputReadLine();
            f.BeginErrorReadLine();
            f.WaitForExit();
            f.Close();
        });
    }

    public async Task FhloaderAsync(string fb)//Fhloader实时输出
    {
        await Task.Run(() =>
        {
            string cmd = Path.Combine(Global.bin_path, "edl", "fh_loader");
            ProcessStartInfo fastboot = null;
            fastboot = new ProcessStartInfo(cmd, fb);
            fastboot.CreateNoWindow = true;
            fastboot.UseShellExecute = false;
            fastboot.RedirectStandardOutput = true;
            fastboot.RedirectStandardError = true;
            Process f = Process.Start(fastboot);
            f.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            f.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            f.BeginOutputReadLine();
            f.BeginErrorReadLine();
            f.WaitForExit();
            f.Close();
        });
    }

    public async void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!String.IsNullOrEmpty(outLine.Data))
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StringBuilder sb = new StringBuilder(EDLLog.Text);
                EDLLog.Text = sb.AppendLine(outLine.Data).ToString();
                EDLLog.CaretIndex = EDLLog.Text.Length;
            });
        }
    }

    private async void OpenImageFile(object sender, RoutedEventArgs args)
    {
        Button button = (Button)sender;
        EDLPartModel eDLPartModel = ( EDLPartModel )button.DataContext;
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Image File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            eDLPartModel.FileName = Path.GetFileName(StringHelper.FilePath(files[0].Path.ToString()));
        }
    }

    private async void BatchWrite(object sender, RoutedEventArgs args)
    {
        string com = Global.thisdevice;
        string elf = FirehoseFile.Text;
        string imgdir = Path.GetDirectoryName(elf);
        string xml = XMLFile.Text;
        string storage = "UFS";
        string shell = String.Format(@"-p \\.\{0} -s 13:{1}", com, elf);
        QSaharaServerAsync(shell);
        shell = String.Format(@"--port=\\.\{0} --search_path={1} --memoryname={2} --noprompt --sendxml={3} --zlpawarehost=1 --reset", com, imgdir, storage, xml);
        FhloaderAsync(shell);
    }
}