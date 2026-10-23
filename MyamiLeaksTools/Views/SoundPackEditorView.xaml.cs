using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace MyamiLeaksTools.Views;

public partial class SoundPackEditorView : UserControl
{
    public SoundPackEditorView() => InitializeComponent();
    private void Primary_Click(object sender, RoutedEventArgs e) { }
    private void Apply_Click(object sender, RoutedEventArgs e)
        => (Application.Current.MainWindow as MainWindow)?.SetStatus("Applied.");
}
