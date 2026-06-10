using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UotanToolbox.Features.Filemgr;

/// <summary>
/// <para>ADB文件列表解析器，用于解析 adb shell ls -la 命令的输出。</para>
/// Parser for ADB file listing output from the "adb shell ls -la" command.
/// </summary>
public static class AdbFileListParser
{
    /// <summary>
    /// <para>用于匹配 ls -la 输出行的正则表达式。</para>
    /// Regex pattern for matching lines from ls -la output.
    /// </summary>
    /// <remarks>
    /// <para>捕获组说明：1=类型字符, 2=权限位(9字符), 3=所有者, 4=所属组, 5=大小, 6=日期, 7=时间, 8=名称部分。</para>
    /// Capture groups: 1=type char, 2=permission bits (9 chars), 3=owner, 4=group, 5=size, 6=date, 7=time, 8=name part.
    /// </remarks>
    private static readonly Regex LineRegex = new(
        @"^([dlcbps-])([rwxsStT-]{9})(?:[.+])?\s+\d+\s+(\S+)\s+(\S+)\s+(\d+)\s+(\d{4}-\d{2}-\d{2})\s+(\d{2}:\d{2})\s+(.+)$",
        RegexOptions.Compiled);

    /// <summary>
    /// <para>解析 adb shell ls -la 命令的输出，返回文件条目列表。</para>
    /// Parses the output of the "adb shell ls -la" command and returns a list of file entries.
    /// </summary>
    /// <param name="output">
    /// <para>adb shell ls -la 命令的原始输出字符串。</para>
    /// The raw output string from the "adb shell ls -la" command.
    /// </param>
    /// <param name="parentPath">
    /// <para>父目录路径，用于构建文件的完整路径。</para>
    /// The parent directory path used to construct the full path of each entry.
    /// </param>
    /// <returns>
    /// <para>排序后的文件条目列表，目录在前、文件在后，各自按名称字母排序。</para>
    /// A sorted list of file entries, with directories first then files, both sorted alphabetically by name.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <para>当 output 为 null 时抛出。</para>
    /// Thrown when output is null.
    /// </exception>
    public static List<FileEntry> Parse(string output, string parentPath)
    {
        ArgumentNullException.ThrowIfNull(output);

        var entries = new List<FileEntry>();

        if (string.IsNullOrEmpty(output))
            return entries;

        string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Skip "total N" summary lines
            if (line.StartsWith("total ", StringComparison.OrdinalIgnoreCase))
                continue;

            Match match = LineRegex.Match(line.Trim());
            if (!match.Success)
                continue;

            string typeChar = match.Groups[1].Value;
            string permissionBits = match.Groups[2].Value;
            string owner = match.Groups[3].Value;
            string group = match.Groups[4].Value;
            long size = long.Parse(match.Groups[5].Value);
            string date = match.Groups[6].Value;
            string time = match.Groups[7].Value;
            string namePart = match.Groups[8].Value.TrimEnd();

            bool isDirectory = typeChar == "d";
            bool isSymlink = typeChar == "l";

            string name;
            string symlinkTarget = string.Empty;

            if (isSymlink)
            {
                int arrowIndex = namePart.IndexOf(" -> ", StringComparison.Ordinal);
                if (arrowIndex >= 0)
                {
                    name = namePart.Substring(0, arrowIndex);
                    symlinkTarget = namePart.Substring(arrowIndex + 4);
                }
                else
                {
                    name = namePart;
                }
            }
            else
            {
                name = namePart;
            }

            // Skip "." and ".." entries
            if (name == "." || name == "..")
                continue;

            string fullPath = ConstructFullPath(parentPath, name);

            var entry = new FileEntry
            {
                Name = name,
                FullPath = fullPath,
                Size = size,
                ModifiedTime = $"{date} {time}",
                IsDirectory = isDirectory,
                IsSymlink = isSymlink,
                SymlinkTarget = symlinkTarget,
                Permission = typeChar + permissionBits,
                Owner = owner,
                Group = group,
                DisplayName = name,
            };

            entries.Add(entry);
        }

        // Sort: directories first, then files, both alphabetically by name
        return entries
            .OrderBy(e => e.IsDirectory ? 0 : 1)
            .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// <para>根据父路径和名称构建完整路径，正确处理尾部斜杠。</para>
    /// Constructs a full path from parent path and name, handling trailing slashes correctly.
    /// </summary>
    /// <param name="parentPath">
    /// <para>父目录路径。</para>
    /// The parent directory path.
    /// </param>
    /// <param name="name">
    /// <para>文件或目录名称。</para>
    /// The file or directory name.
    /// </param>
    /// <returns>
    /// <para>拼接后的完整路径。</para>
    /// The constructed full path.
    /// </returns>
    private static string ConstructFullPath(string parentPath, string name)
    {
        if (string.IsNullOrEmpty(parentPath))
            return "/" + name;

        if (parentPath.EndsWith("/"))
            return parentPath + name;

        return parentPath + "/" + name;
    }
}
