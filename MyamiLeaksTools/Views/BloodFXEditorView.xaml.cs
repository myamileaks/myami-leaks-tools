using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using MyamiLeaksTools.CodeWalkerIntegration;

namespace MyamiLeaksTools.Views;

public partial class BloodFXEditorView : UserControl
{
    private readonly ParticleParser _parser = new();
    private string? _filePath;
    public BloodFXEditorView() => InitializeComponent();

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml" };
        if (dlg.ShowDialog() != true) return;
        _filePath = dlg.FileName;
        UpdateSwatch();
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        if (_filePath == null) return;
        _parser.BatchRecolor(_filePath,
            (float)SlR.Value / 0.6f, (float)SlG.Value / 0.01f,
            (float)SlB.Value / 0.05f, (float)SlA.Value);
        (Application.Current.MainWindow as MainWindow)?.SetStatus("Blood FX applied.");
    }

    private void Save_Click(object sender, RoutedEventArgs e) =>
        (Application.Current.MainWindow as MainWindow)?.SetStatus("Saved.");

    private void PresetNormal_Click(object sender, RoutedEventArgs e) { SlR.Value=0.6; SlG.Value=0.0; SlB.Value=0.05; SlA.Value=1.0; UpdateSwatch(); }
    private void PresetDark_Click(object sender, RoutedEventArgs e)   { SlR.Value=0.35; SlG.Value=0.0; SlB.Value=0.02; SlA.Value=1.0; UpdateSwatch(); }
    private void PresetNeon_Click(object sender, RoutedEventArgs e)   { SlR.Value=0.0; SlG.Value=1.0; SlB.Value=0.2; SlA.Value=1.0; UpdateSwatch(); }
    private void PresetBlack_Click(object sender, RoutedEventArgs e)  { SlR.Value=0.0; SlG.Value=0.0; SlB.Value=0.0; SlA.Value=1.0; UpdateSwatch(); }

    private void UpdateSwatch() =>
        Swatch.Background = new SolidColorBrush(Color.FromArgb(
            (byte)(SlA.Value*255),(byte)(SlR.Value*255),(byte)(SlG.Value*255),(byte)(SlB.Value*255)));
}
