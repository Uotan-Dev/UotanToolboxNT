using System.Diagnostics;
using System.Threading.Tasks;

namespace UotanToolbox.Common
{
    internal class CallExternalProgram
    {
        public static async Task<string> ADB(string adbshell)
        {
            string cmd;
            if (Global.System == "Windows")
            {
                cmd = "bin\\Windows\\platform-tools\\adb.exe";
            }
            else
            {
                cmd = $"bin/{Global.System}/platform-tools/adb";
            }
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
            string cmd;
            if (Global.System == "Windows")
            {
                cmd = "bin\\Windows\\platform-tools\\fastboot.exe";
            }
            else
            {
                cmd = $"bin/{Global.System}/platform-tools/fastboot";
            }
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
            string cmd = "lsusb";
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
            string cmd;
            if (Global.System == "Windows")
            {
                cmd = "bin\\Windows\\platform-tools\\scrcpy.exe";
            }
            else
            {
                cmd = $"bin/{Global.System}/platform-tools/scrcpy";
            }
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
    }
}
