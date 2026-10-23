using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace MyamiLeaksTools.Views;

public partial class TracerEditorView : UserControl
{
    public TracerEditorView() => InitializeComponent();
    private void Primary_Click(object sender, RoutedEventArgs e) { }
    private void Apply_Click(object sender, RoutedEventArgs e)
        => (Application.Current.MainWindow as MainWindow)?.SetStatus("Applied.");
}
