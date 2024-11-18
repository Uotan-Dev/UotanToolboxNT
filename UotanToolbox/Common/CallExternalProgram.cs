using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    internal class CallExternalProgram
    {
        public static async Task<string> ADB(string adbshell)
        {
            string cmd = Path.Combine(Global.bin_path, "platform-tools", "adb");
            ProcessStartInfo adbexe = new ProcessStartInfo(cmd, adbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            using Process adb = new Process();
            adb.StartInfo = adbexe;
            _ = adb.Start();
            string output = await adb.StandardOutput.ReadToEndAsync();
            if (output == "")
            {
                output = await adb.StandardError.ReadToEndAsync();
            }
            adb.WaitForExit();
            return output;
        }

        public static async Task<string> HDC(string hdcshell)
        {
            string cmd = Path.Combine(Global.bin_path, "toolchains", "hdc");
            ProcessStartInfo hdcexe = new ProcessStartInfo(cmd, hdcshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            using Process hdc = new Process();
            hdc.StartInfo = hdcexe;
            hdc.Start();
            string output = await hdc.StandardOutput.ReadToEndAsync();
            if (output == "")
            {
                output = await hdc.StandardError.ReadToEndAsync();
            }
            hdc.WaitForExit();
            return output;
        }

        public static async Task<string> Fastboot(string fbshell)
        {
            string cmd = Path.Combine(Global.bin_path, "platform-tools", "fastboot");
            ProcessStartInfo fastboot = new ProcessStartInfo(cmd, fbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            using Process fb = new Process();
            fb.StartInfo = fastboot;
            _ = fb.Start();
            string output = await fb.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await fb.StandardOutput.ReadToEndAsync();
            }
            fb.WaitForExit();
            return output;
        }

        public static async Task<string> Devcon(string shell)
        {
            string cmd = Path.Combine(Global.bin_path, "devcon");
            ProcessStartInfo fastboot = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            using Process devcon = new Process();
            devcon.StartInfo = fastboot;
            _ = devcon.Start();
            string output = await devcon.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await devcon.StandardOutput.ReadToEndAsync();
            }
            devcon.WaitForExit();
            return output;
        }

        public static async Task<string> Sudo(string shell)
        {
            string cmd = "pkexec";
            ProcessStartInfo fastboot = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            using Process fb = new Process();
            fb.StartInfo = fastboot;
            _ = fb.Start();
            string output = await fb.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await fb.StandardOutput.ReadToEndAsync();
            }
            fb.WaitForExit();
            return output;
        }

        public static async Task<string> QCNTool(string shell)
        {
            string cmd = "Bin\\QSML\\QCNTool.exe";
            ProcessStartInfo qcntool = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            using Process qcn = new Process();
            qcn.StartInfo = qcntool;
            _ = qcn.Start();
            string output = await qcn.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await qcn.StandardOutput.ReadToEndAsync();
            }
            qcn.WaitForExit();
            return output;
        }

        public static async Task<string> Pnputil(string shell)
        {
            string cmd = @"pnputil.exe";
            ProcessStartInfo pnputil = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            using Process pnp = new Process();
            pnp.StartInfo = pnputil;
            _ = pnp.Start();
            string output = await pnp.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await pnp.StandardOutput.ReadToEndAsync();
            }
            return output;
        }

        public static async Task<string> LsUSB()
        {
            string cmd = Global.System == "macOS" ? Path.Combine(Global.bin_path, "lsusb") : "lsusb";
            ProcessStartInfo fastboot = new ProcessStartInfo(cmd)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            using Process fb = new Process();
            fb.StartInfo = fastboot;
            _ = fb.Start();
            string output = await fb.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await fb.StandardOutput.ReadToEndAsync();
            }
            fb.WaitForExit();
            return output;
        }

        public static async Task<string> LinuxLS(string shell)
        {
            string cmd = "ls";
            ProcessStartInfo fastboot = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            using Process fb = new Process();
            fb.StartInfo = fastboot;
            _ = fb.Start();
            string output = await fb.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await fb.StandardOutput.ReadToEndAsync();
            }
            fb.WaitForExit();
            return output;
        }

        public static async Task<string> Scrcpy(string arg)
        {
            string cmd = Path.Combine(Global.bin_path, "platform-tools", "scrcpy");
            Environment.SetEnvironmentVariable("KEEPVERITY", Path.Combine(Global.bin_path, "platform-tools"));
            ProcessStartInfo scrcpy = new ProcessStartInfo(cmd, arg)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                WorkingDirectory = Path.Combine(Global.bin_path, "platform-tools")
            };
            using Process sc = new Process();
            sc.StartInfo = scrcpy;
            _ = sc.Start();
            string output = await sc.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await sc.StandardOutput.ReadToEndAsync();
            }
            sc.WaitForExit();
            return output;
        }
        public static async Task<string> SevenZip(string args)
        {
            string cmd = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(Global.bin_path, "7z", "7za.exe")
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) | (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    ? Path.Combine(Global.bin_path, "7zza")
                    : throw new PlatformNotSupportedException("This function only supports Windows,macOS and Linux.");
            ProcessStartInfo SevenZipexe = new ProcessStartInfo(cmd, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            using Process SevenZip = new Process();
            SevenZip.StartInfo = SevenZipexe;
            _ = SevenZip.Start();
            string output = await SevenZip.StandardOutput.ReadToEndAsync();
            if (output == "")
            {
                output = await SevenZip.StandardError.ReadToEndAsync();
            }
            SevenZip.WaitForExit();
            return output;
        }
        /// <summary>
        /// 调用MagiskBoot执行命令，环境变量由EnvironmentVariable变量设置
        /// </summary>
        /// <param name="shell">需要执行的命令</param>
        /// <param name="workpath">工作目录</param>
        /// <returns>执行结果输出与执行结果代码</returns>
        public static async Task<(string Output, int ExitCode)> MagiskBoot(string shell, string workpath)
        {
            Environment.SetEnvironmentVariable("KEEPVERITY", EnvironmentVariable.KEEPVERITY.ToString().ToLower());
            Environment.SetEnvironmentVariable("KEEPFORCEENCRYPT", EnvironmentVariable.KEEPFORCEENCRYPT.ToString().ToLower());
            Environment.SetEnvironmentVariable("PATCHVBMETAFLAG", EnvironmentVariable.PATCHVBMETAFLAG.ToString().ToLower());
            Environment.SetEnvironmentVariable("RECOVERYMODE", EnvironmentVariable.RECOVERYMODE.ToString().ToLower());
            Environment.SetEnvironmentVariable("LEGACYSAR", EnvironmentVariable.LEGACYSAR.ToString().ToLower());
            string cmd = Path.Combine(Global.bin_path, "magiskboot");
            Directory.SetCurrentDirectory(workpath);
            ProcessStartInfo magiskboot = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8
            };
            Process mb = new Process
            {
                StartInfo = magiskboot
            };
            _ = mb.Start();
            string output = await mb.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await mb.StandardOutput.ReadToEndAsync();
            }
            mb.WaitForExit();
            int exitCode = mb.ExitCode;
            return (output, exitCode);
        }


        /// <summary>
        /// 使用file命令判断文件的类型和指令集。暂不支持FAT Binary多架构检测
        /// </summary>
        /// <param name="filePath">要检查的文件路径。</param>
        /// <returns>file命令的输出结果，包含文件类型和指令集信息。</returns>
        public static async Task<string> File(string path)
        {
            string cmd;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessStartInfo fileInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Global.bin_path, "File", "file.exe"),
                    Arguments = path,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                };
                using Process process = new Process
                {
                    StartInfo = fileInfo,
                    EnableRaisingEvents = true
                };
                _ = process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                return string.IsNullOrEmpty(output) ? error : output;
            }
            else
            {
                ProcessStartInfo fileInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/file",
                    Arguments = path,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using Process process = new Process
                {
                    StartInfo = fileInfo,
                    EnableRaisingEvents = true
                };
                _ = process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                return string.IsNullOrEmpty(output) ? error : output;
            }
        }
        /// <summary>
        /// 调用QSaharaServer执行命令
        /// </summary>
        /// <param name="port">COM Port / TTY device to use. Example /dev/ttyUSB0 or \\\\.\\COM5</param>
        /// <param name="verbose">Print verbose messages</param>
        /// <param name="command">Force Sahara command mode</param>
        /// <param name="memdump">Force Sahara memory debug mode</param>
        /// <param name="image">Force Sahara image transfer mode</param>
        /// <param name="sahara">Specify Sahara protocol file mapping</param>
        /// <param name="prefix">Specify prefix to use on files received from target</param>
        /// <param name="where">Path to store files received from target</param>
        /// <param name="ramdumpimage">Image ID which must be transferred before forcing Sahara memory dump mode</param>
        /// <param name="efssyncloop">At the end of the Sahara state machine loop back to the beginning</param>
        /// <param name="rxtimeout">Timeout for which to wait for data from the port</param>
        /// <param name="maxwrite">Max bytes to write in a single shot to the port</param>
        /// <param name="addsearchpath">Where to look for files</param>
        /// <param name="sendclearstate">Send command to target to reset the Sahara State Machine. Does not work on all targets.</param>
        /// <param name="portnumber">Specify the port number instead of port file. the string \\\\.\\COM will be added as the port.</param>
        /// <param name="switchimagetx">Forcefully put the target into image transfer mode after command execution</param>
        /// <param name="nomodereset">After downloading ram dumps, do not switch to any mode, just return. If enabled returns error after downloading dumps.</param>
        /// <param name="cmdrespfilepath">File name with path to save response received for executed command, cmd</param>
        /// <param name="uart">Sahara protocol over UART</param>
        /// <param name="ethernet">Sahara protocol over ethernet</param>
        /// <param name="ethernet_port">Ethernet/Network port number to listen for connections on when using ethernet interface</param>
        /// <param name="noreset">Option in Android Disable sending the sahara reset PKT</param>
        /// <param name="resettxfail">Option in Android Send reset to client when image transfer fails, resets are not sent if flag -n or -noreset is set</param>
        /// <returns></returns>
        public static async Task<string> QSaharaServer(
            string port = null,
            int? verbose = null,
            string command = null,
            bool memdump = false,
            bool image = false,
            string sahara = null,
            string prefix = null,
            string where = null,
            string ramdumpimage = null,
            bool efssyncloop = false,
            int? rxtimeout = null,
            int? maxwrite = null,
            string addsearchpath = null,
            bool sendclearstate = false,
            int? portnumber = null,
            bool switchimagetx = false,
            bool nomodereset = false,
            string cmdrespfilepath = null,
            bool uart = false,
            bool ethernet = false,
            int? ethernet_port = null,
            bool noreset = false,
            bool resettxfail = false
        )
        {
            string cmd = Path.Combine(Global.bin_path, "QSaharaServer");
            List<string> args = [];

            if (port != null) args.Add($"--port {port}");
            if (verbose.HasValue) args.Add($"--verbose {verbose}");
            if (command != null) args.Add($"--command \"{command}\"");
            if (memdump) args.Add("--memdump");
            if (image) args.Add("--image  ");
            if (sahara != null) args.Add($"--sahara \"{sahara}\"");
            if (prefix != null) args.Add($"--prefix \"{prefix}\"");
            if (where != null) args.Add($"--where \"{where}\"");
            if (ramdumpimage != null) args.Add($"--ramdumpimage \"{ramdumpimage}\"");
            if (efssyncloop) args.Add("--efssyncloop");
            if (rxtimeout.HasValue) args.Add($"--rxtimeout \"{rxtimeout}\"");
            if (maxwrite.HasValue) args.Add($"--maxwrite \"{maxwrite}\"");
            if (addsearchpath != null) args.Add($"--addsearchpath \"{addsearchpath}\"");
            if (sendclearstate) args.Add("--sendclearstate");
            if (portnumber.HasValue) args.Add($"--portnumber \"{portnumber}\"");
            if (switchimagetx) args.Add("--switchimagetx");
            if (nomodereset) args.Add("--nomodereset");
            if (cmdrespfilepath != null) args.Add($"--cmdrespfilepath \"{cmdrespfilepath}\"");
            if (uart) args.Add("--UART");
            if (ethernet) args.Add("--ethernet");
            if (ethernet_port.HasValue) args.Add($"--ethernet_port \"{ethernet_port}\"");
            if (noreset) args.Add("--noreset");
            if (resettxfail) args.Add("--resettxfail");

            ProcessStartInfo adbexe = new ProcessStartInfo(cmd, string.Join(" ", args))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using Process QSS = new Process();
            QSS.StartInfo = adbexe;
            _ = QSS.Start();
            string output = await QSS.StandardOutput.ReadToEndAsync();
            if (output == "")
            {
                output = await QSS.StandardError.ReadToEndAsync();
            }
            QSS.WaitForExit();
            return output;
        }
        public static async Task<string> Fh_Loader(
            bool benchmarkdigestperformance = false,
            bool benchmarkreads = false,
            bool benchmarkwrites = false,
            string createvipdigests = null,
            string chaineddigests = null,
            string contentsxml = null,
            bool convertprogram2read = false,
            string digestsperfilename = null,
            string erase = null,
            string files = null,
            bool firmwarewrite = false,
            string fixgpt = null,
            string flattenbuildto = null,
            string flavor = null,
            bool forcecontentsxmlpaths = false,
            string getstorageinfo = null,
            string json_in = null,
            string labels = null,
            string loglevel = null,
            string lun = null,
            string mainoutputdir = null,
            string maxpayloadsizeinbytes = null,
            string memoryname = null,
            string notfiles = null,
            string notlabels = null,
            bool nop = false,
            bool noprompt = false,
            string num_sectors = null,
            string port = null,
            string start_sector = null,
            bool verbose = false,
            bool verify_programming_getsha = false,
            bool verify_programming = false,
            string zlpawarehost = null,
            bool noautoconfigure = false,
            bool autoconfig = false
            )
        {
            List<string> args = [];
            string cmd = Path.Combine(Global.bin_path, "fh_loader");

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
            if (start_sector != null) args.Add($"--start_sector=\"{start_sector}\"");
            if (verbose) args.Add("--verbose");
            if (verify_programming_getsha) args.Add("--verify_programming_getsha");
            if (verify_programming) args.Add("--verify_programming");
            if (zlpawarehost != null) args.Add($"--zlpawarehost=\"{zlpawarehost}\"");
            if (noautoconfigure) args.Add("--noautoconfigure");
            if (autoconfig) args.Add("--autoconfig");

            ProcessStartInfo adbexe = new ProcessStartInfo(cmd, string.Join(" ", args))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using Process fh_loader = new Process();
            fh_loader.StartInfo = adbexe;
            _ = fh_loader.Start();
            string output = await fh_loader.StandardOutput.ReadToEndAsync();
            if (output == "")
            {
                output = await fh_loader.StandardError.ReadToEndAsync();
            }
            fh_loader.WaitForExit();
            return output;
        }
    }
}
