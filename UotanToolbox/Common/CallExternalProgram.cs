using SukiUI.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Common
{
    internal class CallExternalProgram
    {
        public static async Task<string> ADB(string adbShellCommand, bool debug = false)
        {
            try
            {
                string cmdPath = Path.Combine(Global.bin_path, "platform-tools", "adb");
                using (Process adbProcess = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(cmdPath, adbShellCommand)
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    adbProcess.StartInfo = startInfo;

                    if (!adbProcess.Start())
                    {
                        LogError(debug, "Failed to start ADB process.");
                        return null;
                    }
                    var outputTask = adbProcess.StandardOutput.ReadToEndAsync();
                    var errorTask = adbProcess.StandardError.ReadToEndAsync();
                    await Task.WhenAll(outputTask, errorTask);
                    string output = await Task.Run(() => outputTask.Result.Trim());
                    string error = await Task.Run(() => errorTask.Result.Trim());
                    if (string.IsNullOrEmpty(output))
                    {
                        output = error;
                    }
                    adbProcess.WaitForExit();
                    if (adbProcess.ExitCode != 0)
                    {
                        LogError(debug, $"ADB failed with code {adbProcess.ExitCode}. Command: {adbShellCommand}");
                        return output;
                    }
                    return output;
                }
            }
            catch (Exception ex)
            {
                LogError(debug, ex.Message);
                return null;
            }
        }

        private static void LogError(bool debug, string message)
        {
            if (debug)
            {
                SukiHost.ShowDialog(new ConnectionDialog(message));
            }
        }

        public static async Task<string> Fastboot(string fastbootCommand, bool debug = false)
        {
            try
            {
                string cmdPath = Path.Combine(Global.bin_path, "platform-tools", "fastboot");
                ProcessStartInfo startInfo = new ProcessStartInfo(cmdPath, fastbootCommand)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (Process fastbootProcess = new Process())
                {
                    fastbootProcess.StartInfo = startInfo;

                    if (!fastbootProcess.Start())
                    {
                        if (debug)
                        {
                            SukiHost.ShowDialog(new ConnectionDialog("Failed to start Fastboot process."));
                        }
                        return null;
                    }
                    var outputTask = fastbootProcess.StandardOutput.ReadToEndAsync();
                    var errorTask = fastbootProcess.StandardError.ReadToEndAsync();
                    await Task.WhenAll(outputTask, errorTask);
                    string output = !string.IsNullOrEmpty(await errorTask) ? await errorTask : await outputTask;
                    output = output.Trim();
                    fastbootProcess.WaitForExit();
                    if (fastbootProcess.ExitCode != 0)
                    {
                        if (debug)
                        {
                            SukiHost.ShowDialog(new ConnectionDialog($"Fastboot command execution failed with exit code {fastbootProcess.ExitCode}. Command: {fastbootCommand}"));
                        }
                    }
                    return output;
                }
            }
            catch (Exception ex)
            {
                if (debug)
                {
                    SukiHost.ShowDialog(new ConnectionDialog($"An error occurred in Fastboot operation: {ex.Message}"));
                }
                return "";
            }
        }

        public static async Task<string> Devcon(string shell)
        {
            string cmd = Path.Combine(Global.bin_path, "devcon");
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
            string cmd = "Bin\\QSML\\QCNTool.exe";
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

        public static async Task<string> Pnputil(string shell)
        {
            string cmd = @"pnputil.exe";
            ProcessStartInfo pnputil = new ProcessStartInfo(cmd, shell)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process pnp = new Process();
            pnp.StartInfo = pnputil;
            pnp.Start();
            string output = await pnp.StandardError.ReadToEndAsync();
            if (output == "")
            {
                output = await pnp.StandardOutput.ReadToEndAsync();
            }
            return output;
        }

        public static async Task<string> LsUSB()
        {
            string cmd;
            if (Global.System == "macOS")
            {
                cmd = Path.Combine(Global.bin_path, "lsusb");
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
            string cmd = Path.Combine(Global.bin_path, "platform-tools", "scrcpy");
            ProcessStartInfo scrcpy = new ProcessStartInfo(cmd, arg)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using Process sc = new Process();
            sc.StartInfo = scrcpy;
            sc.Start();
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
            string cmd;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                cmd = Path.Combine(Global.bin_path, "7z", "7za.exe");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) | (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)))
            {
                cmd = Path.Combine(Global.bin_path, "7zza");
            }
            else
            {
                throw new PlatformNotSupportedException("This function only supports Windows,macOS and Linux.");
            }
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
        public static async Task<(string Output, int ExitCode)> MagiskBoot(string shell, string workpath, string KEEPVERITY = "true", string KEEPFORCEENCRYPT = "true", string PATCHVBMETAFLAG = "false", string RECOVERYMODE = "false", string LEGACYSAR = "true")
        {
            try
            {
                Environment.SetEnvironmentVariable("KEEPVERITY", KEEPVERITY);
                Environment.SetEnvironmentVariable("KEEPFORCEENCRYPT", KEEPFORCEENCRYPT);
                Environment.SetEnvironmentVariable("PATCHVBMETAFLAG", PATCHVBMETAFLAG);
                Environment.SetEnvironmentVariable("RECOVERYMODE", RECOVERYMODE);
                Environment.SetEnvironmentVariable("LEGACYSAR", LEGACYSAR);
                string cmd = Path.Combine(Global.bin_path, "magiskboot");
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
                await Task.Run(() =>
                {
                    Thread.Sleep(1000);
                });
                mb.WaitForExit();
                int exitCode = mb.ExitCode; // 获取进程退出代码
                return (output, exitCode);
            }
            catch (Exception ex)
            {
                {
                    SukiHost.ShowDialog(new ConnectionDialog($"An error occurred in Magiskboot operation: {ex.Message}"));
                    return (null, 1);
                }
            }
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
