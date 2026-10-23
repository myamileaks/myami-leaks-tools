using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyamiLeaksTools;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Lazy<UserControl>> _views;

    public MainWindow()
    {
        InitializeComponent();
        _views = new Dictionary<string, Lazy<UserControl>>
        {
            ["Home"]            = new(() => new Views.HomeView(NavigateTo)),
            ["Sky"]             = new(() => new Views.SkyEditorView()),
            ["Timecycle"]       = new(() => new Views.TimecycleEditorView()),
            ["BloodFX"]         = new(() => new Views.BloodFXEditorView()),
            ["RPF"]             = new(() => new Views.RPFEditorView()),
            ["Tracer"]          = new(() => new Views.TracerEditorView()),
            ["Particles"]       = new(() => new Views.ParticlesEditorView()),
            ["SoundPack"]       = new(() => new Views.SoundPackEditorView()),
            ["Stables"]         = new(() => new Views.StablesEditorView()),
            ["Ramps"]           = new(() => new Views.RampsEditorView()),
            ["GenD10"]          = new(() => new Views.GenD10View()),
            ["Snow"]            = new(() => new Views.SnowPacksView()),
            ["AnimSky"]         = new(() => new Views.AnimationSkyView()),
            ["ParticlesCustom"] = new(() => new Views.ParticlesCustomView()),
            ["RPFProtect"]      = new(() => new Views.RPFProtectionView()),
            ["Performance"]     = new(() => new Views.PerformanceView()),
            ["AI"]              = new(() => new Views.AIAssistantView()),
        };
        NavigateTo("Home");
    }

    public void NavigateTo(string key)
    {
        if (!_views.TryGetValue(key, out var view)) return;
        ContentArea.Content = view.Value;
        SetStatus($"Loaded: {key}");
    }

    public void SetStatus(string text, bool ok = true)
    {
        StatusText.Text = text;
        var color = ok
            ? (Color)ColorConverter.ConvertFromString("#30CC70")
            : (Color)ColorConverter.ConvertFromString("#FF1830");
        StatusDot.Fill = new SolidColorBrush(color);
        var glow = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color       = color,
            BlurRadius  = 4,
            ShadowDepth = 0,
            Opacity     = 0.9
        };
        StatusDot.Effect = glow;
    }

    // ── window chrome ────────────────────────────────────────────────
    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void Maximize_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void Close_Click(object sender, RoutedEventArgs e)
        => Close();

    // ── navigation ───────────────────────────────────────────────────
    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string key)
            NavigateTo(key);
    }

    // ── language switch ──────────────────────────────────────────────
    private void LangEN_Click(object sender, RoutedEventArgs e)
        => (Application.Current as App)?.SwitchLanguage("en");
    private void LangES_Click(object sender, RoutedEventArgs e)
        => (Application.Current as App)?.SwitchLanguage("es");
    private void LangFR_Click(object sender, RoutedEventArgs e)
        => (Application.Current as App)?.SwitchLanguage("fr");

    // ── discord link ─────────────────────────────────────────────────
    public ICommand OpenDiscordCommand => new RelayCommand(() =>
        Process.Start(new ProcessStartInfo("https://discord.gg/myami")
            { UseShellExecute = true }));
}

// minimal relay command
file class RelayCommand(Action execute) : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? _) => true;
    public void Execute(object? _) => execute();
}
