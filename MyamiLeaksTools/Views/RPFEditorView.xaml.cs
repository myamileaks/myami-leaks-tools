using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using MyamiLeaksTools.CodeWalkerIntegration;

namespace MyamiLeaksTools.Views;

public partial class RPFEditorView : UserControl
{
    private readonly RPFManager _mgr = new();
    private string? _rpfPath;
    private RpfEntry? _selected;

    public RPFEditorView() => InitializeComponent();

    private async void OpenRPF_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "RPF Archives (*.rpf)|*.rpf|All Files|*.*" };
        if (dlg.ShowDialog() != true) return;

        _rpfPath = dlg.FileName;
        RpfStatus.Text = "Loading...";
        RpfTree.Items.Clear();

        try
        {
            var entries = await _mgr.ListContentsAsync(_rpfPath);
            BuildTree(entries, null);
            RpfStatus.Text = $"Loaded: {Path.GetFileName(_rpfPath)}  ({entries.Count} entries)";
        }
        catch (Exception ex)
        {
            RpfStatus.Text = $"Error: {ex.Message}";
        }
    }

    private void BuildTree(List<RpfEntry> entries, TreeViewItem? parent)
    {
        foreach (var entry in entries)
        {
            var node = new TreeViewItem
            {
                Header = entry.Name,
                Tag    = entry,
            };
            if (entry.IsFolder)
                node.IsExpanded = false;

            if (parent == null) RpfTree.Items.Add(node);
            else parent.Items.Add(node);

            if (entry.Children.Count > 0)
                BuildTree(entry.Children, node);
        }
    }

    private void RpfTree_SelectedItemChanged(object sender,
        RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is not TreeViewItem { Tag: RpfEntry entry }) return;
        _selected = entry;
        EntryName.Text = entry.Name;
        EntryPath.Text = entry.Path;
        EntrySize.Text = entry.IsFolder ? "Directory" : $"{entry.Size:N0} bytes";

        // show first 512 bytes as hex if available
        HexPreview.Text = entry.IsFolder
            ? "[Directory]"
            : $"[Select 'Extract Selected' to view contents]\n\nPath: {entry.Path}\nType: {entry.Type}\nSize: {entry.Size:N0} bytes";
    }

    private async void Extract_Click(object sender, RoutedEventArgs e)
    {
        if (_selected == null || _rpfPath == null) return;

        var dlg = new SaveFileDialog
        {
            FileName = _selected.Name,
            Filter   = "All Files|*.*"
        };
        if (dlg.ShowDialog() != true) return;

        try
        {
            await _mgr.ExtractAsync(_rpfPath, _selected.Path, dlg.FileName);
            RpfStatus.Text = $"Extracted: {_selected.Name}";
        }
        catch (Exception ex) { RpfStatus.Text = $"Error: {ex.Message}"; }
    }

    private async void Inject_Click(object sender, RoutedEventArgs e)
    {
        if (_rpfPath == null) { RpfStatus.Text = "Open an RPF first."; return; }
        if (_selected == null) { RpfStatus.Text = "Select target entry."; return; }

        var dlg = new OpenFileDialog { Title = "Select file to inject" };
        if (dlg.ShowDialog() != true) return;

        try
        {
            await _mgr.InjectAsync(_rpfPath, _selected.Path, dlg.FileName);
            RpfStatus.Text = $"Injected: {Path.GetFileName(dlg.FileName)} → {_selected.Path}";
        }
        catch (Exception ex) { RpfStatus.Text = $"Error: {ex.Message}"; }
    }

    private void SetCwPath_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Locate CodeWalkerCmd.exe",
            Filter = "Executable (*.exe)|*.exe"
        };
        if (dlg.ShowDialog() != true) return;
        RPFManager.CodeWalkerCmdPath = dlg.FileName;
        RpfStatus.Text = $"CodeWalker set: {dlg.FileName}";
    }
}
