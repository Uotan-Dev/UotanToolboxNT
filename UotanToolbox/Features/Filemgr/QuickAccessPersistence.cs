using Material.Icons;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Filemgr;

/// <summary>
/// <para>快捷访问目录项的持久化数据模型，用于 JSON 序列化。</para>
/// Persistence data model for quick access directory items, used for JSON serialization.
/// </summary>
public class CustomQuickAccessData
{
    /// <summary>
    /// <para>快捷访问项的显示名称。</para>
    /// The display name of the quick access item.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// <para>快捷访问项对应的设备目录路径。</para>
    /// The device directory path of the quick access item.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// <para>快捷访问目录的持久化管理器，负责自定义快捷访问项的加载与保存。</para>
/// Persistence manager for quick access directories, handling loading and saving of custom quick access items.
/// </summary>
public static class QuickAccessPersistence
{
    private static readonly string ConfigFileName = "quickaccess.json";

    /// <summary>
    /// <para>获取快捷访问配置文件的完整路径。</para>
    /// Gets the full path of the quick access configuration file.
    /// </summary>
    private static string ConfigFilePath => Path.Combine(Global.runpath, ConfigFileName);

    /// <summary>
    /// <para>从配置文件加载自定义快捷访问项列表。若文件不存在或解析失败，返回空列表。</para>
    /// Loads the custom quick access items list from the configuration file. Returns an empty list if the file does not exist or parsing fails.
    /// </summary>
    /// <returns>
    /// <para>自定义快捷访问项的数据列表。</para>
    /// The data list of custom quick access items.
    /// </returns>
    public static List<CustomQuickAccessData> Load()
    {
        try
        {
            if (!File.Exists(ConfigFilePath))
                return [];

            string json = File.ReadAllText(ConfigFilePath);
            var items = JsonConvert.DeserializeObject<List<CustomQuickAccessData>>(json);
            return items ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// <para>将自定义快捷访问项列表保存到配置文件。</para>
    /// Saves the custom quick access items list to the configuration file.
    /// </summary>
    /// <param name="items">
    /// <para>要保存的自定义快捷访问项数据列表。</para>
    /// The data list of custom quick access items to save.
    /// </param>
    public static void Save(IEnumerable<CustomQuickAccessData> items)
    {
        try
        {
            string json = JsonConvert.SerializeObject(items, Formatting.Indented);
            File.WriteAllText(ConfigFilePath, json);
        }
        catch
        {
            // Silently ignore persistence errors to avoid disrupting user workflow
        }
    }

    /// <summary>
    /// <para>将自定义快捷访问数据转换为 QuickAccessItem 对象，标记为自定义项。</para>
    /// Converts custom quick access data to QuickAccessItem objects, marked as custom items.
    /// </summary>
    /// <param name="dataItems">
    /// <para>自定义快捷访问数据列表。</para>
    /// The list of custom quick access data.
    /// </param>
    /// <returns>
    /// <para>转换后的 QuickAccessItem 列表。</para>
    /// The converted list of QuickAccessItem.
    /// </returns>
    public static List<QuickAccessItem> ToQuickAccessItems(IEnumerable<CustomQuickAccessData> dataItems)
    {
        return dataItems.Select(d => new QuickAccessItem
        {
            Name = d.Name,
            Path = d.Path,
            Icon = MaterialIconKind.Pin,
            IsCustom = true
        }).ToList();
    }

    /// <summary>
    /// <para>将 QuickAccessItem 中的自定义项提取为持久化数据模型。</para>
    /// Extracts custom items from QuickAccessItem into persistence data models.
    /// </summary>
    /// <param name="items">
    /// <para>快捷访问项列表。</para>
    /// The list of quick access items.
    /// </param>
    /// <returns>
    /// <para>自定义项的持久化数据列表。</para>
    /// The persistence data list of custom items.
    /// </returns>
    public static List<CustomQuickAccessData> FromQuickAccessItems(IEnumerable<QuickAccessItem> items)
    {
        return items
            .Where(i => i.IsCustom)
            .Select(i => new CustomQuickAccessData { Name = i.Name, Path = i.Path })
            .ToList();
    }
}
