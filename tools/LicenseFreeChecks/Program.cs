using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: LicenseFreeChecks <repository-root>");
    return 2;
}

var root = Path.GetFullPath(args[0]);
var git = new ProcessStartInfo("git")
{
    WorkingDirectory = root,
    RedirectStandardOutput = true,
    UseShellExecute = false
};
git.ArgumentList.Add("ls-files");
git.ArgumentList.Add("-z");
using var process = Process.Start(git)!;
var output = process.StandardOutput.ReadToEnd();
process.WaitForExit();
if (process.ExitCode != 0)
    return process.ExitCode;

// Ordinal comparison catches casing mistakes even on Windows.
var tracked = output.Split('\0', StringSplitOptions.RemoveEmptyEntries)
    .ToHashSet(StringComparer.Ordinal);
// Unity ignores these names, including every descendant of an ignored folder.
bool IsUnityIgnored(string path) => path.Split('/').Any(name =>
    name.StartsWith(".", StringComparison.Ordinal) ||
    name.EndsWith("~", StringComparison.Ordinal) ||
    name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) ||
    name.Equals("cvs", StringComparison.OrdinalIgnoreCase));
var trackedAssets = tracked.Where(path => path.StartsWith("Assets/", StringComparison.Ordinal)).ToArray();
var assets = trackedAssets
    .Where(path => !IsUnityIgnored(path))
    .OrderBy(path => path, StringComparer.Ordinal).ToArray();
var errors = 0;
void Fail(string message)
{
    Console.Error.WriteLine(message);
    errors++;
}

if (assets.Length == 0)
    Fail("No tracked files under Assets; run against the Unity repository root.");

// Include ignored contents: .gitkeep-only and Samples~-only parents are real folders.
// Git tracks files, so derive folders from all tracked Assets paths, not local directories.
var folders = new HashSet<string>(StringComparer.Ordinal);
foreach (var path in trackedAssets)
{
    var parent = path;
    while ((parent = parent[..parent.LastIndexOf('/')]) != "Assets")
        folders.Add(parent);
    if (!File.Exists(Path.Combine(root, path)))
        Fail($"Tracked file is missing from the working tree: {path}");
}

foreach (var path in assets.Where(path => !path.EndsWith(".meta", StringComparison.Ordinal))
    .Concat(folders.Where(path => !IsUnityIgnored(path))))
{
    if (!tracked.Contains(path + ".meta"))
        Fail($"Missing tracked .meta: {path}.meta");
}

foreach (var meta in assets.Where(path => path.EndsWith(".meta", StringComparison.Ordinal)))
{
    var target = meta[..^5];
    if (!tracked.Contains(target) && !folders.Contains(target))
        Fail($"Orphan .meta: {meta}");
}

var guids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
var guidPattern = new Regex(@"^guid: ([0-9a-fA-F]{32})\s*$", RegexOptions.Multiline);
var metas = tracked.Where(path => path.EndsWith(".meta", StringComparison.Ordinal))
    .Where(path => !IsUnityIgnored(path))
    .OrderBy(path => path, StringComparer.Ordinal).ToArray();
foreach (var meta in metas)
{
    var fullPath = Path.Combine(root, meta);
    if (!File.Exists(fullPath))
    {
        Fail($"Cannot read tracked .meta: {meta}");
        continue;
    }
    var matches = guidPattern.Matches(File.ReadAllText(fullPath));
    if (matches.Count != 1)
    {
        Fail($"Expected exactly one 32-hex GUID: {meta}");
        continue;
    }
    var guid = matches[0].Groups[1].Value;
    if (guids.TryGetValue(guid, out var previous))
        Fail($"Duplicate GUID {guid}: {previous} and {meta}");
    else
        guids.Add(guid, meta);
}

// Unity 2022.3 uses C# 9. No Unity assemblies are needed for this gate.
var options = new CSharpParseOptions(LanguageVersion.CSharp9);
var scripts = assets.Where(path => path.EndsWith(".cs", StringComparison.Ordinal)).ToArray();
var trees = new List<SyntaxTree>();
if (scripts.Length == 0)
    Fail("No tracked C# scripts under Assets.");
foreach (var script in scripts)
{
    var fullPath = Path.Combine(root, script);
    if (!File.Exists(fullPath))
        continue;
    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(fullPath), options, path: script);
    trees.Add(tree);
    foreach (var diagnostic in tree.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error))
        Fail(diagnostic.ToString());
}

// Binding needs BCL types to produce feature-version errors such as >>> and span patterns.
// Use the checker's .NET 8 runtime on Windows and Linux; no Unity assemblies are loaded.
var references = new[]
{
    MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
};
var compilation = CSharpCompilation.Create("LanguageVersionCheck", trees, references);
// Keep this a language-version gate: unresolved Unity types and other semantic errors
// (including CS0246, CS0103 and CS0518) must remain outside this allowlist.
var languageVersionErrors = new HashSet<string>(StringComparer.Ordinal)
{
    "CS8773", "CS8703", "CS8652", "CS8967"
};
foreach (var diagnostic in compilation.GetDiagnostics().Where(item =>
    item.Severity == DiagnosticSeverity.Error && languageVersionErrors.Contains(item.Id)))
    Fail(diagnostic.ToString());

Console.WriteLine($"Checked {scripts.Length} C# scripts, {metas.Length} .meta files, " +
    $"{folders.Count} asset folders: {errors} error(s).");
return errors == 0 ? 0 : 1;
