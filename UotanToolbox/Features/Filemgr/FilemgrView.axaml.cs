using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using SukiUI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UotanToolbox.Features.Filemgr;

public partial class FilemgrView : UserControl
{
    public FilemgrView()
    {
        InitializeComponent();

        // <para>在文件列表卡片上启用外部文件拖入（OS -> 设备 push）。</para>
        // Enable external file drag-in (OS -> device push) on the file list card.
        var dropTarget = this.FindControl<SukiUI.Controls.GlassCard>("FileListCard");
        if (dropTarget is not null)
        {
            dropTarget.AddHandler(DragDrop.DragOverEvent, OnFileListDragOver);
            dropTarget.AddHandler(DragDrop.DropEvent, OnFileListDrop);
        }


    }

    /// <summary>
    /// <para>处理文件条目双击事件，调用 ViewModel 的打开命令。</para>
    /// Handles the file entry double-tap event, invoking the ViewModel's open command.
    /// </summary>
    private void OnFileEntryDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not GlassCard card)
            return;

        var entry = card.DataContext as FileEntry;
        if (entry is null)
            return;

        if (DataContext is FilemgrViewModel vm)
        {
            vm.OpenFileEntryCommand.Execute(entry);
        }
    }

    /// <summary>
    /// <para>拖入悬停：仅当携带文件且设备已连接时显示复制光标。</para>
    /// Drag-over: show copy cursor only when files are carried and the device is connected.
    /// </summary>
    private void OnFileListDragOver(object? sender, DragEventArgs e)
    {
        if (DataContext is not FilemgrViewModel vm)
            return;

        e.DragEffects = vm.IsDeviceConnected && e.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    /// <summary>
    /// <para>拖入放下：收集本机文件/文件夹路径，交由 ViewModel 统一推送（自动适配 HDC 与 Root 模式）。</para>
    /// Drop: collect local file/folder paths and hand them to the ViewModel unified push (auto HDC & root support).
    /// </summary>
    private async void OnFileListDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not FilemgrViewModel vm)
            return;

        if (!e.Data.Contains(DataFormats.Files))
            return;

        var items = e.Data.GetFiles()?.ToList();
        if (items is null || items.Count == 0)
            return;

        var localPaths = new List<string>();
        foreach (var item in items)
        {
            var localPath = item.TryGetLocalPath();
            if (!string.IsNullOrEmpty(localPath) && (File.Exists(localPath) || Directory.Exists(localPath)))
            {
                localPaths.Add(localPath);
            }
        }

        if (localPaths.Count == 0)
            return;

        try
        {
            await vm.PushLocalPathsAsync(localPaths);
        }
        catch (Exception ex)
        {
            global::System.Diagnostics.Trace.WriteLine($"[FilemgrView] drop push failed: {ex.Message}");
        }
    }
}
