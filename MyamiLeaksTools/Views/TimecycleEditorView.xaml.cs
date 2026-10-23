using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace MyamiLeaksTools.Views;

public partial class TimecycleEditorView : UserControl
{
    private string? _filePath;
    public TimecycleEditorView() => InitializeComponent();

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml" };
        if (dlg.ShowDialog() != true) return;
        _filePath = dlg.FileName;
        XmlEditor.Text = File.ReadAllText(_filePath);
        (Application.Current.MainWindow as MainWindow)
            ?.SetStatus($"Loaded: {Path.GetFileName(_filePath)}");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_filePath == null) return;
        File.WriteAllText(_filePath, XmlEditor.Text);
        (Application.Current.MainWindow as MainWindow)?.SetStatus("Saved.");
    }

    private void Weather_Changed(object sender, SelectionChangedEventArgs e) { }
}
