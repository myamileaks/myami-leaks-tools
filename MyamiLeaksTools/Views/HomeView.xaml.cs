using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MyamiLeaksTools.Views;

public partial class HomeView : UserControl
{
    private readonly Action<string> _navigate;

    private record ToolDef(string Key, string Icon, string Name, string Desc, string Group);

    private static readonly ToolDef[] Tools =
    [
        new("Sky",             "◎", "Sky Editor",         "Edit zenith/nadir sky gradient per weather state",       "VISUAL"),
        new("Timecycle",       "◑", "Timecycle Editor",   "Full timecycle_v.xml editor with lighting sliders",      "VISUAL"),
        new("AnimSky",         "▷", "Animation Sky",      "Animated sky sequences for dynamic weather transitions",  "VISUAL"),
        new("Stables",         "⬡", "Stables Editor",     "3D model preview and texture replacement for stables",   "VISUAL"),
        new("BloodFX",         "◈", "Blood FX Editor",    "Recolor blood & gore particle effects with presets",     "EFFECTS"),
        new("Particles",       "✦", "Particles Editor",   "Batch recolor all RGBA particle XML entries",            "EFFECTS"),
        new("Tracer",          "⟶", "Tracer Editor",      "Bullet tracer texture and color customization",          "EFFECTS"),
        new("ParticlesCustom", "✧", "Particles Custom",   "Per-pack custom particle color profiles",                "EFFECTS"),
        new("Snow",            "❅", "Snow for All Packs", "Apply snow color multipliers to all particle XMLs",      "EFFECTS"),
        new("RPF",             "⊞", "RPF Editor",         "Browse, extract and inject files inside RPF8 archives",  "FILES"),
        new("RPFProtect",      "⊡", "RPF Protection",     "Encrypt & obfuscate RPF archives via CodeWalker",        "FILES"),
        new("GenD10",          "⚄", "Gen D10 Packs",      "Randomly generate D10 color variant packs",              "FILES"),
        new("Ramps",           "⧲", "Ramps Editor",       "Customize color ramp textures for vehicle shaders",      "FILES"),
        new("SoundPack",       "♪", "SoundPack Editor",   "Browse and replace FiveM audio bank entries",            "AUDIO"),
        new("Performance",     "⚡", "Performance",        "Analyze and optimize particle XML complexity",           "PERFORMANCE"),
        new("AI",              "◈", "Tokkyo AI",          "Ask Tokkyo AI for GTA V modding guidance",               "AI"),
    ];

    private static readonly Dictionary<string, string> GroupColors = new()
    {
        ["VISUAL"]      = "#0C1018",
        ["EFFECTS"]     = "#100C0C",
        ["FILES"]       = "#0C0C14",
        ["AUDIO"]       = "#0A100C",
        ["PERFORMANCE"] = "#100E08",
        ["AI"]          = "#0C0C18",
    };

    private static readonly Dictionary<string, string> GroupAccents = new()
    {
        ["VISUAL"]      = "#1E3050",
        ["EFFECTS"]     = "#3C0810",
        ["FILES"]       = "#1E1E40",
        ["AUDIO"]       = "#103020",
        ["PERFORMANCE"] = "#382808",
        ["AI"]          = "#1E1048",
    };

    public HomeView(Action<string> navigate)
    {
        _navigate = navigate;
        InitializeComponent();
        BuildCards();
    }

    private void BuildCards()
    {
        foreach (var tool in Tools)
        {
            GroupColors.TryGetValue(tool.Group, out var bg);
            GroupAccents.TryGetValue(tool.Group, out var accent);

            var card = new Border
            {
                Width           = 210,
                Height          = 110,
                Margin          = new Thickness(5),
                CornerRadius    = new CornerRadius(12),
                BorderThickness = new Thickness(1),
                BorderBrush     = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(accent ?? "#1E1E28")),
                Padding = new Thickness(14, 12),
                Cursor  = Cursors.Hand,
                Effect  = new DropShadowEffect
                {
                    Color       = Colors.Black,
                    BlurRadius  = 10,
                    ShadowDepth = 3,
                    Opacity     = 0.5
                }
            };

            card.Background = new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString(bg ?? "#111115"),
                (Color)ColorConverter.ConvertFromString("#0C0C10"),
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1));

            var iconBorder = new Border
            {
                Width           = 32,
                Height          = 32,
                CornerRadius    = new CornerRadius(8),
                Background      = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(accent ?? "#1E1E28")),
                Margin          = new Thickness(0, 0, 0, 8),
                Child = new TextBlock
                {
                    Text                = tool.Icon,
                    FontSize            = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center,
                    Foreground          = new SolidColorBrush(Colors.White)
                }
            };

            var nameText = new TextBlock
            {
                Text       = tool.Name,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize   = 12.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#F4F4F8")),
                Margin     = new Thickness(0, 0, 0, 3),
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            var descText = new TextBlock
            {
                Text         = tool.Desc,
                FontFamily   = new FontFamily("Segoe UI"),
                FontSize     = 10.5,
                Foreground   = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#505070")),
                TextWrapping = TextWrapping.Wrap,
                MaxHeight    = 32
            };

            var stack = new StackPanel();
            stack.Children.Add(iconBorder);
            stack.Children.Add(nameText);
            stack.Children.Add(descText);
            card.Child = stack;

            // hover effect
            var baseAccentColor = (Color)ColorConverter.ConvertFromString(accent ?? "#1E1E28");
            card.MouseEnter += (_, _) =>
            {
                card.BorderBrush = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#CC1020"));
                card.Effect = new DropShadowEffect
                {
                    Color       = (Color)ColorConverter.ConvertFromString("#CC1020"),
                    BlurRadius  = 12,
                    ShadowDepth = 0,
                    Opacity     = 0.3
                };
            };
            card.MouseLeave += (_, _) =>
            {
                card.BorderBrush = new SolidColorBrush(baseAccentColor);
                card.Effect = new DropShadowEffect
                {
                    Color = Colors.Black, BlurRadius = 10, ShadowDepth = 3, Opacity = 0.5
                };
            };

            // navigate on click
            var toolKey = tool.Key;
            card.MouseLeftButtonUp += (_, _) => _navigate(toolKey);

            ToolsGrid.Items.Add(card);
        }
    }
}
