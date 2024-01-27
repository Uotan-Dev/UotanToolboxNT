using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SukiUI.Demo.Common
{
    internal class ADBHelper
    {
        public static async Task<string> ADB(string adbshell)
        {
            string cmd = "bin\\adb\\adb.exe";
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
            string cmd = "bin\\adb\\fastboot.exe";
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
    }
}
