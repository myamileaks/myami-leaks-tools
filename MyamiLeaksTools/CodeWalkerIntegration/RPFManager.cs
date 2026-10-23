using System.Diagnostics;
using System.IO;

namespace MyamiLeaksTools.CodeWalkerIntegration;

// ── RPF entry model ───────────────────────────────────────────────

public class RpfEntry
{
    public string Name     { get; set; } = "";
    public string Path     { get; set; } = "";
    public long   Size     { get; set; }
    public bool   IsFolder { get; set; }
    public string Type     { get; set; } = "";  // e.g. "ytd", "ymap", "xml", ...
    public List<RpfEntry> Children { get; set; } = [];
}

// ── RPFManager — wraps CodeWalker CLI for RPF operations ──────────
//
//  To use full CodeWalker integration:
//    1. Clone https://github.com/dexyfex/CodeWalker
//    2. Build CodeWalkerCmd project
//    3. Set CodeWalkerCmdPath below to the built .exe path
//    4. Or add CodeWalker as a project reference and call its API directly

public class RPFManager
{
    // Path to CodeWalkerCmd.exe — set via Settings or auto-detect
    public static string CodeWalkerCmdPath { get; set; } =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CodeWalker", "CodeWalkerCmd.exe");

    private static bool CwAvailable =>
        File.Exists(CodeWalkerCmdPath);

    // ── list RPF contents ─────────────────────────────────────────

    public async Task<List<RpfEntry>> ListContentsAsync(string rpfPath)
    {
        if (CwAvailable)
            return await ListViaCwAsync(rpfPath);

        return await ListViaSharpCompress(rpfPath);
    }

    private async Task<List<RpfEntry>> ListViaCwAsync(string rpfPath)
    {
        var output = await RunCwAsync($"list \"{rpfPath}\"");
        var entries = new List<RpfEntry>();

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split('|');
            if (parts.Length < 3) continue;
            entries.Add(new RpfEntry
            {
                Path     = parts[0].Trim(),
                Name     = Path.GetFileName(parts[0].Trim()),
                Size     = long.TryParse(parts[1].Trim(), out var sz) ? sz : 0,
                Type     = Path.GetExtension(parts[0].Trim()).TrimStart('.').ToLower(),
                IsFolder = parts[2].Trim() == "DIR"
            });
        }
        return entries;
    }

    private async Task<List<RpfEntry>> ListViaSharpCompress(string rpfPath)
    {
        // Fallback: read RPF7/RPF8 header manually
        // RPF8 magic: 0x52504638
        await using var fs  = File.OpenRead(rpfPath);
        using var       br  = new BinaryReader(fs);
        uint magic = br.ReadUInt32();

        if (magic != 0x52504638 && magic != 0x38465052)
            throw new InvalidDataException("Not a valid RPF file (magic mismatch).");

        // Full RPF parsing requires CodeWalker — show helpful message
        return
        [
            new RpfEntry
            {
                Name     = $"[CodeWalker not found — set path in Settings]",
                Path     = rpfPath,
                Size     = new FileInfo(rpfPath).Length,
                Type     = "rpf",
                IsFolder = true
            }
        ];
    }

    // ── extract file from RPF ─────────────────────────────────────

    public async Task ExtractAsync(string rpfPath, string internalPath, string outPath)
    {
        if (!CwAvailable)
            throw new InvalidOperationException(
                "CodeWalker not found. Clone https://github.com/dexyfex/CodeWalker and set the path in Settings.");

        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        await RunCwAsync($"extract \"{rpfPath}\" \"{internalPath}\" \"{outPath}\"");
    }

    // ── inject file into RPF ──────────────────────────────────────

    public async Task InjectAsync(string rpfPath, string internalPath, string srcPath)
    {
        if (!CwAvailable)
            throw new InvalidOperationException("CodeWalker not found.");

        await RunCwAsync($"inject \"{rpfPath}\" \"{internalPath}\" \"{srcPath}\"");
    }

    // ── apply RPF protection (obfuscate entry names + encrypt) ───

    public async Task<string> ProtectAsync(string rpfPath, string outDir)
    {
        if (!CwAvailable)
            throw new InvalidOperationException("CodeWalker required for RPF protection.");

        string outPath = Path.Combine(outDir, Path.GetFileNameWithoutExtension(rpfPath) + "_protected.rpf");
        await RunCwAsync($"protect \"{rpfPath}\" \"{outPath}\"");
        return outPath;
    }

    // ── CodeWalkerCmd runner ──────────────────────────────────────

    private static async Task<string> RunCwAsync(string args)
    {
        var psi = new ProcessStartInfo(CodeWalkerCmdPath, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
        };

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start CodeWalkerCmd.");

        string stdout = await proc.StandardOutput.ReadToEndAsync();
        string stderr = await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();

        if (proc.ExitCode != 0 && !string.IsNullOrEmpty(stderr))
            throw new Exception($"CodeWalker error: {stderr}");

        return stdout;
    }
}
