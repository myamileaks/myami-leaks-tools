using System.IO;
using System.Xml.Linq;

namespace MyamiLeaksTools.CodeWalkerIntegration;

// ── Sky / timecycle data types ───────────────────────────────────

public record SkyColors
{
    public string Zenith { get; init; } = "#1A1A4A";
    public string Nadir  { get; init; } = "#0A0A1E";
    public string East   { get; init; } = "#2A1A3A";
    public string West   { get; init; } = "#3A1A2A";
}

public static class SkyPresets
{
    private static readonly Dictionary<string, SkyColors> _presets = new()
    {
        ["EXTRASUNNY"] = new() { Zenith="#1A3A6A", Nadir="#0A1A3A", East="#2A4A6A", West="#3A2A1A" },
        ["CLEAR"]      = new() { Zenith="#1E3A60", Nadir="#0E1E38", East="#243A5A", West="#30241A" },
        ["CLOUDS"]     = new() { Zenith="#2A2A3A", Nadir="#141420", East="#26263A", West="#2A2228" },
        ["SMOG"]       = new() { Zenith="#3A3028", Nadir="#1A1810", East="#383028", West="#362A20" },
        ["FOGGY"]      = new() { Zenith="#303038", Nadir="#181820", East="#2E2E36", West="#302E34" },
        ["OVERCAST"]   = new() { Zenith="#242428", Nadir="#101012", East="#222228", West="#242224" },
        ["RAIN"]       = new() { Zenith="#1A1C20", Nadir="#0C0E10", East="#181C20", West="#1C181E" },
        ["THUNDER"]    = new() { Zenith="#0C0C10", Nadir="#060608", East="#0C0C12", West="#0E0C10" },
        ["SNOW"]       = new() { Zenith="#C0C8D0", Nadir="#A0A8B0", East="#C4C8D0", West="#C0C4CC" },
        ["BLIZZARD"]   = new() { Zenith="#A0A8B0", Nadir="#8090A0", East="#A4A8B0", West="#A0A4AC" },
        ["HALLOWEEN"]  = new() { Zenith="#1A0A00", Nadir="#0A0500", East="#280A00", West="#1A0800" },
        ["XMAS"]       = new() { Zenith="#B0C8E0", Nadir="#90A8C0", East="#B4C8E0", West="#B0C4DC" },
        ["NEUTRAL"]    = new() { Zenith="#2A2A2A", Nadir="#141414", East="#282828", West="#2A2828" },
    };

    public static SkyColors Get(string name) =>
        _presets.TryGetValue(name, out var c) ? c : _presets["EXTRASUNNY"];
}

// ── Parser ────────────────────────────────────────────────────────

public class TimecycleParser
{
    // Reads sky zenith/nadir/east/west from a GTA V timecycle_v.xml
    public SkyColors ParseSkyColors(string xmlPath)
    {
        var doc   = XDocument.Load(xmlPath);
        var root  = doc.Root ?? throw new InvalidDataException("Empty XML");

        // timecycle_v.xml → <CTimeCycleData> → <Mods> → first <CTimeCycleModData>
        // sky colour fields: skyColourZenith, skyColourNadir, skyColourE, skyColourW
        string zenith = ExtractColorHex(root, "skyColourZenith") ?? "#1A1A4A";
        string nadir  = ExtractColorHex(root, "skyColourNadir")  ?? "#0A0A1E";
        string east   = ExtractColorHex(root, "skyColourE")       ?? "#2A1A3A";
        string west   = ExtractColorHex(root, "skyColourW")       ?? "#3A1A2A";

        return new SkyColors { Zenith=zenith, Nadir=nadir, East=east, West=west };
    }

    public void WriteSkyColors(string xmlPath, SkyColors colors)
    {
        // if file doesn't exist or isn't a timecycle, create a stub
        XDocument doc;
        try   { doc = XDocument.Load(xmlPath); }
        catch { doc = BuildStubTimecycle(); }

        var root = doc.Root!;
        SetColorHex(root, "skyColourZenith", colors.Zenith);
        SetColorHex(root, "skyColourNadir",  colors.Nadir);
        SetColorHex(root, "skyColourE",       colors.East);
        SetColorHex(root, "skyColourW",       colors.West);

        doc.Save(xmlPath);
    }

    // ── helpers ──────────────────────────────────────────────────

    private static string? ExtractColorHex(XElement root, string fieldName)
    {
        // <fieldName> <r value="0.1"/> <g value="0.1"/> <b value="0.3"/> </fieldName>
        var el = root.Descendants(fieldName).FirstOrDefault();
        if (el == null) return null;

        float r = float.Parse(el.Element("r")?.Attribute("value")?.Value ?? "0");
        float g = float.Parse(el.Element("g")?.Attribute("value")?.Value ?? "0");
        float b = float.Parse(el.Element("b")?.Attribute("value")?.Value ?? "0");

        int ri = (int)(Math.Clamp(r, 0, 1) * 255);
        int gi = (int)(Math.Clamp(g, 0, 1) * 255);
        int bi = (int)(Math.Clamp(b, 0, 1) * 255);

        return $"#{ri:X2}{gi:X2}{bi:X2}";
    }

    private static void SetColorHex(XElement root, string fieldName, string hex)
    {
        var (r, g, b) = HexToFloat(hex);
        var el = root.Descendants(fieldName).FirstOrDefault();
        if (el == null)
        {
            el = new XElement(fieldName,
                new XElement("r", new XAttribute("value", r.ToString("F6"))),
                new XElement("g", new XAttribute("value", g.ToString("F6"))),
                new XElement("b", new XAttribute("value", b.ToString("F6"))));
            root.Descendants("CTimeCycleModData").FirstOrDefault()?.Add(el);
            return;
        }
        el.Element("r")!.SetAttributeValue("value", r.ToString("F6"));
        el.Element("g")!.SetAttributeValue("value", g.ToString("F6"));
        el.Element("b")!.SetAttributeValue("value", b.ToString("F6"));
    }

    private static (float r, float g, float b) HexToFloat(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length < 6) return (0, 0, 0);
        int ri = Convert.ToInt32(hex[..2], 16);
        int gi = Convert.ToInt32(hex[2..4], 16);
        int bi = Convert.ToInt32(hex[4..6], 16);
        return (ri / 255f, gi / 255f, bi / 255f);
    }

    private static XDocument BuildStubTimecycle() => XDocument.Parse(@"
<CTimeCycleData>
  <Mods>
    <CTimeCycleModData>
      <modName value=""EXTRASUNNY""/>
      <skyColourZenith><r value=""0.1""/><g value=""0.1""/><b value=""0.29""/></skyColourZenith>
      <skyColourNadir><r value=""0.04""/><g value=""0.04""/><b value=""0.12""/></skyColourNadir>
      <skyColourE><r value=""0.16""/><g value=""0.1""/><b value=""0.23""/></skyColourE>
      <skyColourW><r value=""0.23""/><g value=""0.1""/><b value=""0.16""/></skyColourW>
    </CTimeCycleModData>
  </Mods>
</CTimeCycleData>");
}
