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
            var cmd = Path.Combine(Global.bin_path, "platform-tools", "adb");

            var adbexe = new ProcessStartInfo(cmd, adbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var adb = new Process();
            adb.StartInfo = adbexe;
            _ = adb.Start();
            var output = await adb.StandardOutput.ReadToEndAsync();

            if (output == "")
            {
                output = await adb.StandardError.ReadToEndAsync();
            }

            adb.WaitForExit();
            return output;
        }

        public static async Task<string> Fastboot(string fbshell)
        {
            var cmd = Path.Combine(Global.bin_path, "platform-tools", "fastboot");

            var fastboot = new ProcessStartInfo(cmd, fbshell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var fb = new Process();
            fb.StartInfo = fastboot;
            _ = fb.Start();
            var output = await fb.StandardError.ReadToEndAsync();

            if (output == "")
            {
                output = await fb.StandardOutput.ReadToEndAsync();
            }

            fb.WaitForExit();
            return output;
        }

        public static async Task<string> Devcon(string shell)
        {
            var cmd = Path.Combine(Global.bin_path, "devcon");

            var fastboot = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var devcon = new Process();
            devcon.StartInfo = fastboot;
            _ = devcon.Start();
            var output = await devcon.StandardError.ReadToEndAsync();

            if (output == "")
            {
                output = await devcon.StandardOutput.ReadToEndAsync();
            }

            devcon.WaitForExit();
            return output;
        }

        public static async Task<string> QCNTool(string shell)
        {
            var cmd = "Bin\\QSML\\QCNTool.exe";

            var qcntool = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var qcn = new Process();
            qcn.StartInfo = qcntool;
            _ = qcn.Start();
            var output = await qcn.StandardError.ReadToEndAsync();

            if (output == "")
            {
                output = await qcn.StandardOutput.ReadToEndAsync();
            }

            qcn.WaitForExit();
            return output;
        }

        public static async Task<string> Pnputil(string shell)
        {
            var cmd = @"pnputil.exe";

            var pnputil = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var pnp = new Process();
            pnp.StartInfo = pnputil;
            _ = pnp.Start();
            var output = await pnp.StandardError.ReadToEndAsync();

            if (output == "")
            {
                output = await pnp.StandardOutput.ReadToEndAsync();
            }

            return output;
        }

        public static async Task<string> LsUSB()
        {
            var cmd = Global.System == "macOS" ? Path.Combine(Global.bin_path, "lsusb") : "lsusb";

            var fastboot = new ProcessStartInfo(cmd)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var fb = new Process();
            fb.StartInfo = fastboot;
            _ = fb.Start();
            var output = await fb.StandardError.ReadToEndAsync();

            if (output == "")
            {
                output = await fb.StandardOutput.ReadToEndAsync();
            }

            fb.WaitForExit();
            return output;
        }

        public static async Task<string> LinuxLS(string shell)
        {
            var cmd = "ls";

            var fastboot = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var fb = new Process();
            fb.StartInfo = fastboot;
            _ = fb.Start();
            var output = await fb.StandardError.ReadToEndAsync();

            if (output == "")
            {
                output = await fb.StandardOutput.ReadToEndAsync();
            }

            fb.WaitForExit();
            return output;
        }

        public static async Task<string> Scrcpy(string arg)
        {
            var cmd = Path.Combine(Global.bin_path, "platform-tools", "scrcpy");

            var scrcpy = new ProcessStartInfo(cmd, arg)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var sc = new Process();
            sc.StartInfo = scrcpy;
            _ = sc.Start();
            var output = await sc.StandardError.ReadToEndAsync();

            if (output == "")
            {
                output = await sc.StandardOutput.ReadToEndAsync();
            }

            sc.WaitForExit();
            return output;
        }

        public static async Task<string> SevenZip(string args)
        {
            var cmd = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(Global.bin_path, "7z", "7za.exe")
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) | (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    ? Path.Combine(Global.bin_path, "7zza")
                    : throw new PlatformNotSupportedException("This function only supports Windows,macOS and Linux.");

            var SevenZipexe = new ProcessStartInfo(cmd, args)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var SevenZip = new Process();
            SevenZip.StartInfo = SevenZipexe;
            _ = SevenZip.Start();
            var output = await SevenZip.StandardOutput.ReadToEndAsync();

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
            var cmd = Path.Combine(Global.bin_path, "magiskboot");
            Directory.SetCurrentDirectory(workpath);

            var magiskboot = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var mb = new Process
            {
                StartInfo = magiskboot
            };

            _ = mb.Start();
            var output = await mb.StandardError.ReadToEndAsync();

            if (output == "")
            {
                output = await mb.StandardOutput.ReadToEndAsync();
            }

            mb.WaitForExit();
            var exitCode = mb.ExitCode;
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
                var fileInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Global.bin_path, "File", "file.exe"),
                    Arguments = path,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = new Process
                {
                    StartInfo = fileInfo,
                    EnableRaisingEvents = true
                };

                _ = process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                return string.IsNullOrEmpty(output) ? error : output;
            }
            else
            {
                var fileInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/file",
                    Arguments = path,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = new Process
                {
                    StartInfo = fileInfo,
                    EnableRaisingEvents = true
                };

                _ = process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                return string.IsNullOrEmpty(output) ? error : output;
            }
        }
    }
}