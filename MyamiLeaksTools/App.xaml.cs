using System.Windows;
using System.Windows.Threading;

namespace MyamiLeaksTools;

public partial class App : Application
{
    public static string CurrentLanguage { get; private set; } = "en";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnUnhandledException;
    }

    public static void SwitchLanguage(string lang)
    {
        CurrentLanguage = lang;
        var dict = new ResourceDictionary
        {
            Source = new Uri($"Localization/Strings.{lang}.xaml", UriKind.Relative)
        };

        var existing = Current.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains("Localization/Strings") == true);
        if (existing != null)
            Current.Resources.MergedDictionaries.Remove(existing);

        Current.Resources.MergedDictionaries.Add(dict);
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Error: {e.Exception.Message}", "Myami Leaks Tools",
            MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
