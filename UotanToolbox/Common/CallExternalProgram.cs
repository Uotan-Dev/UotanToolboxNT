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
                cmd = "bin\\Windows\\adb\\adb.exe";
            }
            else
            {
                cmd = $"bin/{Global.System}/adb/adb";
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
                cmd = "bin\\Windows\\adb\\fastboot.exe";
            }
            else
            {
                cmd = $"bin/{Global.System}/adb/fastboot";
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
            string cmd = "bin\\Windows\\QCNTool.exe";
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
                cmd = "bin\\Windows\\adb\\scrcpy.exe";
            }
            else
            {
                cmd = $"bin/{Global.System}/adb/scrcpy";
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
        public static async Task<string> SevenZip(string args)
        {
            string cmd;
            if (Global.System == "Windows")
            {
                cmd = "bin\\Windows\\7z\\7z.exe";
            }
            else
            {
                cmd = $"bin/{Global.System}/7z/7zzs";
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
    }
}
