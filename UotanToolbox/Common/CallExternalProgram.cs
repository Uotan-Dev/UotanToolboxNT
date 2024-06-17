using System;
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
                RedirectStandardError = true
            };
            using Process adb = new Process();
            adb.StartInfo = adbexe;
            adb.Start();
            string output = await adb.StandardOutput.ReadToEndAsync();
            if (output == "")
            {
                output = await adb.StandardError.ReadToEndAsync();
            }
            adb.WaitForExit();
            return output;
        }

        public static async Task<string> Fastboot(string fbshell)
        {
            string cmd=Path.Combine(Global.bin_path, "platform-tools","fastboot");
            ProcessStartInfo fastboot = new ProcessStartInfo(cmd, fbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process fb = new Process();
            fb.StartInfo = fastboot;
            fb.Start();
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
            string cmd = "bin\\Windows\\devcon.exe";
            ProcessStartInfo fastboot = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process devcon = new Process();
            devcon.StartInfo = fastboot;
            devcon.Start();
            string output = await devcon.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await devcon.StandardOutput.ReadToEndAsync();
            }
            devcon.WaitForExit();
            return output;
        }
        public static async Task<string> QCNTool(string shell)
        {
            string cmd = "bin\\Windows\\qsml\\QCNTool.exe";
            ProcessStartInfo qcntool = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process qcn = new Process();
            qcn.StartInfo = qcntool;
            qcn.Start();
            string output = await qcn.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await qcn.StandardOutput.ReadToEndAsync();
            }
            qcn.WaitForExit();
            return output;
        }

        public static async Task<string> LsUSB()
        {
            string cmd;
            if (Global.System == "macOS")
            {
                cmd = "bin/macOS/lsusb";
            }
            else
            {
                cmd = "lsusb";
            }
            ProcessStartInfo fastboot = new ProcessStartInfo(cmd)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process fb = new Process();
            fb.StartInfo = fastboot;
            fb.Start();
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
                RedirectStandardError = true
            };
            using Process fb = new Process();
            fb.StartInfo = fastboot;
            fb.Start();
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
            string cmd=Path.Combine(Global.bin_path, "platform-tools", "scrcpy");
            ProcessStartInfo fastboot = new ProcessStartInfo(cmd, arg)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process scrcpy = new Process();
            scrcpy.StartInfo = fastboot;
            scrcpy.Start();
            string output = await scrcpy.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await scrcpy.StandardOutput.ReadToEndAsync();
            }
            scrcpy.WaitForExit();
            return output;
        }
        public static async Task<string> SevenZip(string args)
        {
            string cmd;
            cmd = $"bin/{Global.System}/{(Global.System == "Windows" ? "7z\\7za.exe" : Global.System == "MacOS" ? "7zz" : "7zzs")}";
            ProcessStartInfo SevenZipexe = new ProcessStartInfo(cmd, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process SevenZip = new Process();
            SevenZip.StartInfo = SevenZipexe;
            SevenZip.Start();
            string output = await SevenZip.StandardOutput.ReadToEndAsync();
            if (output == "")
            {
                output = await SevenZip.StandardError.ReadToEndAsync();
            }
            SevenZip.WaitForExit();
            return output;
        }
        public static async Task<(string Output, int ExitCode)> MagiskBoot(string shell,string workpath)
        {
            string cmd=Path.Combine(Global.bin_path,"magiskboot");
            Directory.SetCurrentDirectory(workpath);
            ProcessStartInfo magiskboot = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process mb = new Process();
            mb.StartInfo = magiskboot;
            mb.Start();
            string output = await mb.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await mb.StandardOutput.ReadToEndAsync();
            }
            mb.WaitForExit();
            int exitCode = mb.ExitCode; // 获取进程退出代码
            return (output, exitCode);
        }
    
        /// <summary>
        /// 使用file命令判断文件的类型和指令集。暂不支持FAT Binary多架构检测
        /// </summary>
        /// <param name="filePath">要检查的文件路径。</param>
        /// <returns>file命令的输出结果，包含文件类型和指令集信息。</returns>
        /// <exception cref="FileNotFoundException">当指定的文件路径不存在时抛出。</exception>
        /// <summary>
        /// 调用file命令读取文件信息。
        /// </summary>
        /// <param name="filePath">要分析的文件路径。</param>
        /// <returns>file命令的输出，包含文件的类型和相关信息。</returns>
        public static async Task<string> File(string shell)
        {
            if (shell == null) throw new ArgumentNullException(nameof(shell));
            string cmd;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                cmd = Path.Combine(Global.bin_path, "File", "file.exe");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) | (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)))
            {
                cmd = "file";
            }
            else
            {
                throw new PlatformNotSupportedException("This function only supports Windows,macOS and Linux.");
            }
            ProcessStartInfo fileinfo = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process fi = new Process();
            fi.StartInfo = fileinfo;
            fi.Start();
            string output = await fi.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await fi.StandardOutput.ReadToEndAsync();
            }
            fi.WaitForExit();
            return output;
        }
    }
}
