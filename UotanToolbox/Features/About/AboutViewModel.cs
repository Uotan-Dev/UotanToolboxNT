using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Newtonsoft.Json;
using SukiUI.Dialogs;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UotanToolbox.Common;
using UotanToolbox.Features.Settings;
using UotanToolbox.Utilities;

namespace UotanToolbox.Features.About;

public partial class AboutViewModel : MainPageBase
{
    [ObservableProperty] private string _currentVersion = Global.currentVersion;
    [ObservableProperty] private string _binVersion = null;
    [ObservableProperty] private string _mouZei = "@某贼\r\n提供开发思路";
    [ObservableProperty] private string _kCN = "@剧毒的KCN\r\n安装器开发";
    [ObservableProperty] private string _aCA = "@小太阳ACA\r\n依赖支持";

    private static string GetTranslation(string key)
    {
        return FeaturesHelper.GetTranslation(key);
    }

    public AboutViewModel() : base(GetTranslation("Sidebar_About"), MaterialIconKind.InfoBoxOutline, 10000)
    {
        _ = CheckBinVersion();
    }

    [RelayCommand]
    private void OpenURL(string url)
    {
        UrlUtilities.OpenURL(url);
    }

    public async Task CheckBinVersion()
    {
        BinVersion = await StringHelper.GetBinVersion();
    }

    [RelayCommand]
    private async Task GetUpdate()
    {
        try
        {
            using HttpClient client = new HttpClient();
            string url = "https://toolbox.uotan.cn/api/list";
            StringContent content = new StringContent("{}", System.Text.Encoding.UTF8);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            _ = response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            dynamic convertedBody = JsonConvert.DeserializeObject<dynamic>(responseBody);
            AboutViewModel vm = new AboutViewModel();
            string version = convertedBody.release_version;
            if (version.Contains("beta"))
            {
                if (convertedBody.beta_version != vm.CurrentVersion)
                {
                    string serializedContent = (String)JsonConvert.SerializeObject(convertedBody.beta_content).Replace("\\n", "\n");
                    if (serializedContent.Length > 1) serializedContent = serializedContent.Substring(1, serializedContent.Length - 2);
                    Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Settings_NewVersionAvailable"))
                    .WithContent(serializedContent)
                    .OfType(NotificationType.Information)
                    .WithActionButton(GetTranslation("ConnectionDialog_GetUpdate"), _ => UrlUtilities.OpenURL("https://toolbox.uotan.cn"), true)
                    .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                    .TryShow();
                }
                else Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("Settings_UpToDate")).Dismiss().ByClickingBackground().TryShow();
            }
            else
            {
                if (convertedBody.release_version != vm.CurrentVersion)
                {
                    string serializedContent = (String)JsonConvert.SerializeObject(convertedBody.release_content).Replace("\\n", "\n");
                    if (serializedContent.Length > 1) serializedContent = serializedContent.Substring(1, serializedContent.Length - 2);
                    Global.MainDialogManager.CreateDialog()
                    .WithTitle(GetTranslation("Settings_NewVersionAvailable"))
                    .WithContent(serializedContent)
                    .OfType(NotificationType.Information)
                    .WithActionButton(GetTranslation("ConnectionDialog_GetUpdate"), _ => UrlUtilities.OpenURL("https://toolbox.uotan.cn"), true)
                    .WithActionButton(GetTranslation("ConnectionDialog_Cancel"), _ => { }, true)
                    .TryShow();
                }
                else Global.MainDialogManager.CreateDialog().WithTitle(GetTranslation("Common_Succ")).OfType(NotificationType.Success).WithContent(GetTranslation("Settings_UpToDate")).Dismiss().ByClickingBackground().TryShow();
            }
        }
        catch (HttpRequestException e)
        {
            Global.MainDialogManager.CreateDialog().OfType(NotificationType.Error).WithTitle(GetTranslation("Common_Error")).WithActionButton(GetTranslation("Common_Know"), _ => { }, true).WithContent(e.Message).TryShow();
        }
    }
}