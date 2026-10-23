using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using MyamiLeaksTools.CodeWalkerIntegration;

namespace MyamiLeaksTools.Views;

public partial class SkyEditorView : UserControl
{
    private string? _filePath;
    private bool    _refreshing;

    public SkyEditorView() => InitializeComponent();

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml" };
        if (dlg.ShowDialog() != true) return;
        _filePath   = dlg.FileName;
        var colors  = TimecycleParser.ParseSkyColors(_filePath);
        _refreshing = true;
        TbZenith.Text = colors.Zenith;
        TbNadir.Text  = colors.Nadir;
        TbEast.Text   = colors.East;
        _refreshing = false;
        RefreshPreview();
        (Application.Current.MainWindow as MainWindow)
            ?.SetStatus($"Loaded: {Path.GetFileName(_filePath)}");
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_filePath == null) { Open_Click(sender, e); return; }
        var colors = new SkyColors
        {
            Zenith = TbZenith.Text.Trim(),
            Nadir  = TbNadir.Text.Trim(),
            East   = TbEast.Text.Trim(),
            West   = "#888888"
        };
        TimecycleParser.WriteSkyColors(_filePath, colors);
        (Application.Current.MainWindow as MainWindow)?.SetStatus("Saved timecycle.");
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _refreshing   = true;
        TbZenith.Text = "#1A3A8A";
        TbNadir.Text  = "#E8A030";
        TbEast.Text   = "#C86018";
        _refreshing   = false;
        RefreshPreview();
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
        => Save_Click(sender, e);

    private void Color_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_refreshing) return;
        RefreshSwatch(TbZenith.Text, SwatchZenith);
        RefreshSwatch(TbNadir.Text, SwatchNadir);
        RefreshSwatch(TbEast.Text, SwatchEast);
        RefreshPreview();
    }

    private void Preset_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (PresetList.SelectedItem is not ListBoxItem item) return;
        string preset = item.Content?.ToString() ?? "";
        if (!TimecycleParser.SkyPresets.TryGetValue(preset, out var colors) || colors == null) return;
        _refreshing   = true;
        TbZenith.Text = colors.Zenith;
        TbNadir.Text  = colors.Nadir;
        TbEast.Text   = colors.East;
        _refreshing   = false;
        RefreshSwatch(TbZenith.Text, SwatchZenith);
        RefreshSwatch(TbNadir.Text, SwatchNadir);
        RefreshSwatch(TbEast.Text, SwatchEast);
        RefreshPreview();
    }

    private void Swatch_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not Border swatch) return;
        var initial = TryParseColor(swatch.Background is SolidColorBrush scb
            ? $"#{scb.Color.R:X2}{scb.Color.G:X2}{scb.Color.B:X2}"
            : "#888888");
        var dlg = new ColorPickerDialog(initial) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() != true) return;
        var hexVal = $"#{dlg.PickedColor.R:X2}{dlg.PickedColor.G:X2}{dlg.PickedColor.B:X2}";
        switch (swatch.Tag?.ToString())
        {
            case "Zenith": TbZenith.Text = hexVal; break;
            case "Nadir":  TbNadir.Text  = hexVal; break;
            case "East":   TbEast.Text   = hexVal; break;
        }
    }

    private void RefreshSwatch(string hex, Border swatch)
    {
        try { swatch.Background = new SolidColorBrush(TryParseColor(hex)); }
        catch { /* invalid hex, ignore */ }
    }

    private void RefreshPreview()
    {
        try
        {
            ZenithStop.Color = TryParseColor(TbZenith.Text);
            NadirStop.Color  = TryParseColor(TbNadir.Text);
        }
        catch { /* ignore bad input */ }
    }

    private static Color TryParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            byte r = Convert.ToByte(hex[..2], 16);
            byte g = Convert.ToByte(hex[2..4], 16);
            byte b = Convert.ToByte(hex[4..6], 16);
            return Color.FromRgb(r, g, b);
        }
        return Colors.Gray;
    }
}
