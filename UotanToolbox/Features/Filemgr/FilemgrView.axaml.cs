using Avalonia.Controls;
using Avalonia.Input;
using SukiUI.Controls;

namespace UotanToolbox.Features.Filemgr;

public partial class FilemgrView : UserControl
{
    public FilemgrView()
    {
        InitializeComponent();
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
}
