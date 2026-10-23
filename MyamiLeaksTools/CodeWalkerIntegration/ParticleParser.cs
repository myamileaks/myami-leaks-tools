using System.IO;
using System.Xml.Linq;

namespace MyamiLeaksTools.CodeWalkerIntegration;

// ── Particle color entry ──────────────────────────────────────────

public record ParticleColorEntry
{
    public string EffectName  { get; init; } = "";
    public string FieldName   { get; init; } = "";   // RGBAMinKFP / RGBAMaxKFP / ptxu_Colour
    public float  R, G, B, A;

    public string HexMin => $"#{(int)(R*255):X2}{(int)(G*255):X2}{(int)(B*255):X2}";
}

// ── Parser ────────────────────────────────────────────────────────

public class ParticleParser
{
    public List<ParticleColorEntry> ParseColors(string xmlPath)
    {
        var doc  = XDocument.Load(xmlPath);
        var list = new List<ParticleColorEntry>();

        foreach (var effect in doc.Descendants("Item").Where(e => e.Attribute("type") != null))
        {
            string name = effect.Element("name")?.Value ?? "";

            foreach (var kfp in effect.Descendants("RGBAMinKFP").Concat(
                                       effect.Descendants("RGBAMaxKFP")).Concat(
                                       effect.Descendants("ptxu_Colour")))
            {
                var entry = ParseKFP(kfp, name);
                if (entry != null) list.Add(entry);
            }
        }
        return list;
    }

    public void WriteColors(string xmlPath, IEnumerable<ParticleColorEntry> entries)
    {
        var doc = XDocument.Load(xmlPath);

        foreach (var entry in entries)
        {
            var effect = doc.Descendants("Item")
                .FirstOrDefault(e => e.Element("name")?.Value == entry.EffectName);
            if (effect == null) continue;

            var kfp = effect.Descendants(entry.FieldName).FirstOrDefault();
            if (kfp == null) continue;

            // update first key frame value
            var frame = kfp.Descendants("Item").FirstOrDefault();
            if (frame == null) continue;

            var rgba = frame.Element("colour");
            if (rgba == null)
            {
                rgba = new XElement("colour");
                frame.Add(rgba);
            }
            rgba.SetAttributeValue("r", entry.R.ToString("F6"));
            rgba.SetAttributeValue("g", entry.G.ToString("F6"));
            rgba.SetAttributeValue("b", entry.B.ToString("F6"));
            rgba.SetAttributeValue("a", entry.A.ToString("F6"));
        }

        doc.Save(xmlPath);
    }

    private static ParticleColorEntry? ParseKFP(XElement kfp, string effectName)
    {
        try
        {
            var frame = kfp.Descendants("Item").FirstOrDefault();
            if (frame == null) return null;

            var rgba = frame.Element("colour");
            if (rgba == null) return null;

            float r = float.Parse(rgba.Attribute("r")?.Value ?? "0");
            float g = float.Parse(rgba.Attribute("g")?.Value ?? "0");
            float b = float.Parse(rgba.Attribute("b")?.Value ?? "0");
            float a = float.Parse(rgba.Attribute("a")?.Value ?? "1");

            return new ParticleColorEntry
            {
                EffectName = effectName,
                FieldName  = kfp.Name.LocalName,
                R = r, G = g, B = b, A = a
            };
        }
        catch { return null; }
    }

    // ── batch recolor (for Snow / Custom Particles) ───────────────

    public void BatchRecolor(string xmlPath,
                             float rMul, float gMul, float bMul, float aMul,
                             string? fieldFilter = null)
    {
        var doc = XDocument.Load(xmlPath);
        var targets = doc.Descendants("RGBAMinKFP")
                         .Concat(doc.Descendants("RGBAMaxKFP"))
                         .Concat(doc.Descendants("ptxu_Colour"));

        if (fieldFilter != null)
            targets = targets.Where(e => e.Name.LocalName == fieldFilter);

        foreach (var kfp in targets)
        {
            foreach (var frame in kfp.Descendants("Item"))
            {
                var rgba = frame.Element("colour");
                if (rgba == null) continue;

                float r = float.Parse(rgba.Attribute("r")?.Value ?? "0");
                float g = float.Parse(rgba.Attribute("g")?.Value ?? "0");
                float b = float.Parse(rgba.Attribute("b")?.Value ?? "0");
                float a = float.Parse(rgba.Attribute("a")?.Value ?? "1");

                rgba.SetAttributeValue("r", Math.Clamp(r * rMul, 0, 1).ToString("F6"));
                rgba.SetAttributeValue("g", Math.Clamp(g * gMul, 0, 1).ToString("F6"));
                rgba.SetAttributeValue("b", Math.Clamp(b * bMul, 0, 1).ToString("F6"));
                rgba.SetAttributeValue("a", Math.Clamp(a * aMul, 0, 1).ToString("F6"));
            }
        }
        doc.Save(xmlPath);
    }

    // ── inject snow (turns all particle colours white/blue-white) ─

    public void ApplySnow(string xmlPath, float intensity = 1.0f)
    {
        float snowR = 0.9f * intensity;
        float snowG = 0.95f * intensity;
        float snowB = 1.0f * intensity;
        BatchRecolor(xmlPath, snowR / 0.5f, snowG / 0.5f, snowB / 0.5f, 1.0f);
    }
}
