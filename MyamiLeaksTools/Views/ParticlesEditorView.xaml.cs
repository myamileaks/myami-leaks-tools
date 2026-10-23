using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using MyamiLeaksTools.CodeWalkerIntegration;

namespace MyamiLeaksTools.Views;

public partial class ParticlesEditorView : UserControl
{
    private readonly ParticleParser _parser = new();
    private List<ParticleColorEntry> _entries = [];
    private string? _filePath;

    public ParticlesEditorView() => InitializeComponent();

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml|All Files|*.*" };
        if (dlg.ShowDialog() != true) return;

        _filePath = dlg.FileName;
        try
        {
            _entries = _parser.ParseColors(_filePath);
            ParticleGrid.ItemsSource = _entries;
            (Application.Current.MainWindow as MainWindow)
                ?.SetStatus($"Loaded {_entries.Count} entries from {Path.GetFileName(_filePath)}");
        }
        catch (Exception ex)
        {
            (Application.Current.MainWindow as MainWindow)?.SetStatus(ex.Message, false);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_filePath == null) return;
        try
        {
            _parser.WriteColors(_filePath, _entries);
            (Application.Current.MainWindow as MainWindow)?.SetStatus("Saved.");
        }
        catch (Exception ex)
        {
            (Application.Current.MainWindow as MainWindow)?.SetStatus(ex.Message, false);
        }
    }

    private void BatchRecolor_Click(object sender, RoutedEventArgs e)
    {
        if (_filePath == null) return;
        _parser.BatchRecolor(_filePath, (float)SlR.Value * 2, (float)SlG.Value * 2, (float)SlB.Value * 2, 1.0f);
        _entries = _parser.ParseColors(_filePath);
        ParticleGrid.ItemsSource = _entries;
        (Application.Current.MainWindow as MainWindow)?.SetStatus("Batch recolor applied.");
    }

    private void Search_Changed(object sender, TextChangedEventArgs e)
    {
        string q = SearchBox.Text.ToLower();
        ParticleGrid.ItemsSource = string.IsNullOrEmpty(q)
            ? _entries
            : _entries.Where(x => x.EffectName.ToLower().Contains(q) ||
                                  x.FieldName.ToLower().Contains(q)).ToList();
    }

    private void ParticleGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ParticleGrid.SelectedItem is not ParticleColorEntry entry) return;
        SlR.Value = entry.R;
        SlG.Value = entry.G;
        SlB.Value = entry.B;
        SlA.Value = entry.A;
        UpdateSwatch();
    }

    private void Channel_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        => UpdateSwatch();

    private void UpdateSwatch() =>
        ColorSwatch.Background = new SolidColorBrush(Color.FromArgb(
            (byte)(SlA.Value * 255), (byte)(SlR.Value * 255),
            (byte)(SlG.Value * 255), (byte)(SlB.Value * 255)));

    private void ApplyToSelection_Click(object sender, RoutedEventArgs e)
    {
        if (ParticleGrid.SelectedItem is not ParticleColorEntry old) return;
        var idx = _entries.IndexOf(old);
        if (idx < 0) return;

        _entries[idx] = old with
        {
            R = (float)SlR.Value,
            G = (float)SlG.Value,
            B = (float)SlB.Value,
            A = (float)SlA.Value,
        };
        ParticleGrid.Items.Refresh();
    }
}
