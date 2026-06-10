using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UotanToolbox.Common.Devices
{
    public class HdcTransport : IDeviceTransport
    {
        private static string GetTranslation(string key)
        {
            return FeaturesHelper.GetTranslation(key);
        }

        public TransportType Type => TransportType.Hdc;

        public async Task<IEnumerable<DeviceInfo>> ProbeAsync(CancellationToken cancel = default)
        {
            string output = await CallExternalProgram.HDC("list targets");
            var ids = StringHelper.HDCDevices(output);
            var hids = ids.Select(id => new DeviceInfo(id, TransportType.Hdc));
            if (Global.System.Contains("Linux") && Global.root)
            {
                string devcon = await CallExternalProgram.LsUSB();
                if (devcon.Contains("HDC Device") && ids.Length == 0)
                {
                    Global.MainDialogManager.CreateDialog()
                        .WithTitle(GetTranslation("Common_Warn"))
                        .WithContent(GetTranslation("Common_HDCRoot"))
                        .OfType(NotificationType.Warning)
                        .WithActionButton(GetTranslation("ConnectionDialog_Confirm"), async _ =>
                        {
                            await CallExternalProgram.Sudo("chmod -R 777 /dev/bus/usb/");
                            hids = ids.Select(id => new DeviceInfo(id, TransportType.Hdc));
                        }, true)
                        .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                        .TryShow();
                    Global.root = false;
                }
            }
            return hids;
        }

        public Task<string> RunAsync(DeviceInfo device, string command, CancellationToken cancel = default, Action<string>? outputCallback = null)
        {
            string args = command.TrimStart().StartsWith("-t ", System.StringComparison.Ordinal)
                ? command
                : $"-t {device.Id} {command}";
            return CallExternalProgram.HDC(args, outputCallback);
        }

        public Task<bool> ClaimAsync(DeviceInfo device) => Task.FromResult(true);
        public Task ReleaseAsync(DeviceInfo device) => Task.CompletedTask;
    }
}