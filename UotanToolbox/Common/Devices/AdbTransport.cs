using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UotanToolbox.Common.Devices
{
    public class AdbTransport : IDeviceTransport
    {
        private static string GetTranslation(string key)
        {
            return FeaturesHelper.GetTranslation(key);
        }

        public TransportType Type => TransportType.Adb;

        public async Task<IEnumerable<DeviceInfo>> ProbeAsync(CancellationToken cancel = default)
        {
            string output = await CallExternalProgram.ADB("devices");
            var result = new List<DeviceInfo>();
            var lines = output.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (Global.System.Contains("Linux") && Global.root)
            {
                if (output.Contains("failed to check server version: cannot connect to daemon"))
                {
                    Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Warn"))
                        .WithContent(GetTranslation("Common_ADBRoot"))
                        .OfType(NotificationType.Warning)
                        .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                        {
                            await CallExternalProgram.Sudo("chmod -R 777 /dev/bus/usb/");
                            output = await CallExternalProgram.ADB("devices");
                            var lines = output.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                        }, true)
                        .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                        .TryShow();
                    Global.root = false;
                }

            }

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }

                if (line.StartsWith("List of devices attached"))
                {
                    continue;
                }

                // Ignore adb daemon startup diagnostics, e.g.:
                // "* daemon not running; starting now at tcp:5037"
                // "* daemon started successfully"
                if (trimmed.StartsWith("*", StringComparison.Ordinal))
                {
                    continue;
                }

                var parts = trimmed.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    continue;
                }

                var id = parts[0];
                var state = parts[1];
                var properties = new Dictionary<string, string>
                {
                    ["State"] = state
                };

                result.Add(new DeviceInfo(id, TransportType.Adb, properties));
            }

            return result;
        }

        public Task<string> RunAsync(DeviceInfo device, string command, CancellationToken cancel = default, Action<string>? outputCallback = null)
        {
            string args = command.TrimStart().StartsWith("-s ", System.StringComparison.Ordinal)
                ? command
                : $"-s {device.Id} {command}";
            return CallExternalProgram.ADB(args, outputCallback);
        }

        public Task<bool> ClaimAsync(DeviceInfo device)
        {
            // adb does not require explicit claim; placeholder for future locking
            return Task.FromResult(true);
        }

        public Task ReleaseAsync(DeviceInfo device)
        {
            // nothing to do
            return Task.CompletedTask;
        }
    }
}