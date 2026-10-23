using System.Windows;
using System.Windows.Media;

namespace MyamiLeaksTools.Views;

public partial class ColorPickerDialog : Window
{
    public Color PickedColor { get; private set; }

    private bool _updating;

    public ColorPickerDialog(Color initial)
    {
        InitializeComponent();
        _updating    = true;
        SliderR.Value = initial.R;
        SliderG.Value = initial.G;
        SliderB.Value = initial.B;
        HexBox.Text  = $"{initial.R:X2}{initial.G:X2}{initial.B:X2}";
        _updating    = false;
        RefreshSwatch();
    }

    private void Channel_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_updating) return;
        _updating = true;
        HexBox.Text = $"{(int)SliderR.Value:X2}{(int)SliderG.Value:X2}{(int)SliderB.Value:X2}";
        _updating = false;
        RefreshSwatch();
    }

    private void Hex_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_updating || HexBox.Text.Length != 6) return;
        try
        {
            _updating     = true;
            byte r        = Convert.ToByte(HexBox.Text[..2], 16);
            byte g        = Convert.ToByte(HexBox.Text[2..4], 16);
            byte b        = Convert.ToByte(HexBox.Text[4..6], 16);
            SliderR.Value = r;
            SliderG.Value = g;
            SliderB.Value = b;
            _updating     = false;
            RefreshSwatch();
        }
        catch { _updating = false; }
    }

    private void RefreshSwatch()
    {
        var c = Color.FromRgb((byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value);
        PreviewSwatch.Background = new SolidColorBrush(c);
        // update drop shadow glow color
        PreviewSwatch.Effect = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color       = c,
            BlurRadius  = 12,
            ShadowDepth = 0,
            Opacity     = 0.5
        };
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        PickedColor = Color.FromRgb((byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value);
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}
