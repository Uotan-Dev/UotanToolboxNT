using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
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


    private async void OpenFirehoseFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = new[] { Firehose },
            Title = "Open File",
            AllowMultiple = true
        });
        if (files.Count >= 1)
        {
            FirehoseFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    private async void OpenXMLFile(object sender, RoutedEventArgs args)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = new[] { RawProgramXML },
            Title = "Open File",
            AllowMultiple = true
        });
        if (files.Count >= 1)
        {
            for (int i = 0; i < files.Count; i++)
            {
                if (i != files.Count - 1)
                {
                    XMLFile.Text += String.Format("{0},", Path.GetFileName(StringHelper.FilePath(files[i].Path.ToString())));
                }
                else
                {
                    XMLFile.Text += String.Format("{0}", Path.GetFileName(StringHelper.FilePath(files[i].Path.ToString())));
                }
            }
        }
        TopLevel topLevel2 = TopLevel.GetTopLevel(this);
        System.Collections.Generic.IReadOnlyList<IStorageFile> files2 = await topLevel2.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = new[] { PatchXML },
            Title = "Open File",
            AllowMultiple = true
        });
        for (int i = 0; i < files2.Count; i++)
        {
            XMLFile.Text += String.Format(",{0}", Path.GetFileName(StringHelper.FilePath(files2[i].Path.ToString())));
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