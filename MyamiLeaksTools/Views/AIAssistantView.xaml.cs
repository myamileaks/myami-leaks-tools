using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyamiLeaksTools.Views;

public partial class AIAssistantView : UserControl
{
    public AIAssistantView()
    {
        InitializeComponent();
        AddBubble("assistant", "Tokkyo AI ready. Ask about GTA V modding, particle recolor, timecycle editing, RPF structure, or FiveM pack configuration.");
    }

    private void Send_Click(object sender, RoutedEventArgs e) => SendMessage();
    private void Input_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift)) SendMessage();
    }

    private void SendMessage()
    {
        string msg = InputBox.Text.Trim();
        if (string.IsNullOrEmpty(msg)) return;
        InputBox.Text = "";
        AddBubble("user", msg);
        var reply = GetLocalReply(msg);
        AddBubble("assistant", reply);
        ChatScroll.ScrollToBottom();
    }

    private void AddBubble(string role, string text)
    {
        bool isUser = role == "user";

        var userBg = new LinearGradientBrush(
            (Color)ColorConverter.ConvertFromString("#4A0810"),
            (Color)ColorConverter.ConvertFromString("#2C0408"),
            new Point(0, 0), new Point(1, 1));

        var aiBg = new LinearGradientBrush(
            (Color)ColorConverter.ConvertFromString("#1A1A22"),
            (Color)ColorConverter.ConvertFromString("#111118"),
            new Point(0, 0), new Point(1, 1));

        var bubble = new Border
        {
            CornerRadius        = new CornerRadius(12, 12, isUser ? 2 : 12, isUser ? 12 : 2),
            Padding             = new Thickness(14, 10),
            Margin              = new Thickness(isUser ? 80 : 0, 4, isUser ? 0 : 80, 4),
            HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Background          = isUser ? (Brush)userBg : aiBg,
            BorderBrush         = new SolidColorBrush(
                isUser
                    ? (Color)ColorConverter.ConvertFromString("#6A1018")
                    : (Color)ColorConverter.ConvertFromString("#1E1E2C")),
            BorderThickness     = new Thickness(1),
        };
        bubble.Child = new TextBlock
        {
            Text         = text,
            Foreground   = new SolidColorBrush(Colors.White),
            FontSize     = 13,
            TextWrapping = TextWrapping.Wrap,
            FontFamily   = new FontFamily("Segoe UI"),
        };
        ChatHistory.Children.Add(bubble);
    }

    // ── Local knowledge base ──────────────────────────────────────
    private static string GetLocalReply(string q)
    {
        q = q.ToLower();
        if (q.Contains("rgba") || q.Contains("particle") || q.Contains("color") || q.Contains("colour"))
            return "To recolor particles: open the Particles Editor, load your ptfx XML, select entries with RGBAMinKFP or RGBAMaxKFP fields, and adjust R/G/B/A sliders. Batch Recolor applies a multiplier to all entries at once. Values are normalized 0–1 floats.";
        if (q.Contains("timecycle") || q.Contains("sky") || q.Contains("weather"))
            return "timecycle_v.xml controls GTA V weather and sky appearance per weather state. skyColourZenith/Nadir set the gradient. Open it in the Sky Editor or Timecycle Editor. Changes take effect when you swap weather in-game with SetWeatherTypeNow().";
        if (q.Contains("rpf"))
            return "RPF archives require CodeWalker for full read/write. Clone https://github.com/dexyfex/CodeWalker, build CodeWalkerCmd, then set its path in the RPF Editor toolbar. Once set, you can list, extract, and inject files into any RPF8 archive.";
        if (q.Contains("snow"))
            return "The Snow Packs tool applies white/blue-white particle color overrides to all particle XMLs in a folder. It multiplies RGBAMinKFP and RGBAMaxKFP towards (0.9, 0.95, 1.0) — simulating snow on existing effects.";
        if (q.Contains("fivem") || q.Contains("stream"))
            return "For FiveM streaming: place edited XMLs in stream/ folder of your resource, declare them in fxmanifest.lua under data_file entries. Timecycle mods use type 'TIMECYCLE_FILE', particles use 'PTXD_FILE'.";
        if (q.Contains("blood") || q.Contains("gore"))
            return "Blood FX: find ped_blood_* entries in the particle XML (usually gore_effects.xml in x64c.rpf). The RGBAMinKFP field controls base color. Set R≈0.6, G≈0, B≈0.05 for realistic dark blood. The Blood FX Editor handles this directly.";
        if (q.Contains("protect") || q.Contains("encrypt"))
            return "RPF Protection uses CodeWalker to re-encrypt the archive and obfuscate internal paths. This prevents casual extraction with tools like OpenIV. Note: determined reversers can still extract at runtime.";
        if (q.Contains("tracer") || q.Contains("bullet"))
            return "Tracer colors are set via particle XML or texture replacement. Load your weapons_tracer.ytd in the Tracer Editor, pick a tracer entry, and use the color toolbar at the bottom to set RGB + hex value. Export PNG to replace the texture.";
        if (q.Contains("stables") || q.Contains("horse") || q.Contains("model"))
            return "Stables Editor shows 3D model previews over a perspective grid. Load the stable .ydr, swap textures via the texture list on the right, then export or re-inject into the source RPF.";
        if (q.Contains("gen") || q.Contains("d10") || q.Contains("random"))
            return "Gen D10 Packs randomly generates D10 livery color variants. Set the base color seed and variation range, then click Generate to produce N unique color profiles. Output goes to the selected folder as numbered XMLs.";
        if (q.Contains("ramp") || q.Contains("gradient"))
            return "Color ramps are gradient textures used by vehicle shaders. The Ramps Editor lets you pick colors, build the gradient, preview it, and export as a PNG ready to inject into vehicle meta archives.";
        if (q.Contains("sound") || q.Contains("audio") || q.Contains("awc"))
            return "SoundPack Editor connects to AWC (Audio Wave Container) files. Browse categories (weapons, vehicles, ambient), search entries, and replace .wav files while preserving the AWC structure.";
        if (q.Contains("performance") || q.Contains("optimize") || q.Contains("fps"))
            return "The Performance tool scans particle XMLs for expensive emitters (high maxCount, complex force fields, overlapping effects) and suggests simplifications. Apply optimizations per-file or batch the entire pack folder.";
        return "Ask about: particles · timecycles · RPF archives · FiveM streaming · snow packs · blood FX · tracers · stables · D10 generation · ramps · sounds · performance optimization.";
    }
}
