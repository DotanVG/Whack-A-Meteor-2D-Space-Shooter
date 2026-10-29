using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Release builds omit development tooling without changing Editor import settings.</summary>
public sealed class McpPlayerAssemblyFilter : IFilterBuildAssemblies
{
    public int callbackOrder => 0;
    public string[] OnFilterAssemblies(BuildOptions options, string[] assemblies)
    {
        if (Environment.GetEnvironmentVariable("WAM_INCLUDE_MCP") == "1") return assemblies;
        var tooling = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (Directory.Exists("Assets/Plugins/NuGet"))
            foreach (string path in Directory.GetFiles("Assets/Plugins/NuGet", "*.dll", SearchOption.AllDirectories))
                tooling.Add(Path.GetFileName(path));
        var kept = assemblies.Where(path => !tooling.Contains(Path.GetFileName(path)) &&
            !Path.GetFileName(path).StartsWith("com.IvanMurzak.Unity.MCP.", StringComparison.OrdinalIgnoreCase) &&
            Path.GetFileName(path) != "Extensions.Unity.PlayerPrefsEx.dll").ToArray();
        foreach (string removed in assemblies.Except(kept)) Debug.Log("[WAM Build] Excluded tooling: " + removed);
        return kept;
    }
}

public static class WebGLBuild
{
    [Serializable] private sealed class AssemblyConstraints { public string[] defineConstraints; }

    public static void Build()
    {
        string label = Environment.GetEnvironmentVariable("WAM_BUILD_LABEL") ?? "release";
        if (label.Any(c => !char.IsLetterOrDigit(c) && c != '-')) throw new ArgumentException("Invalid build label");
        string output = "Builds/WebGL-" + label;
        // Mirror the resolver's runtime constraints for this build only. Never persist generated symbols.
        string asmdef = "Packages/com.ivanmurzak.unity.mcp/Runtime/com.IvanMurzak.Unity.MCP.Runtime.asmdef";
        var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(asmdef);
        string[] constraints = package == null ? new string[0] : JsonUtility.FromJson<AssemblyConstraints>(
            File.ReadAllText(Path.Combine(package.resolvedPath, "Runtime/com.IvanMurzak.Unity.MCP.Runtime.asmdef"))).defineConstraints;
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            extraScriptingDefines = constraints,
            target = BuildTarget.WebGL, locationPathName = output, options = BuildOptions.None
        });
        Debug.Log("[WAM Build] Result=" + report.summary.result + " bytes=" + report.summary.totalSize);
        if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("WebGL build failed");
        foreach (string path in Directory.GetFiles(output, "*", SearchOption.AllDirectories))
            Debug.Log("[WAM Size] " + path + " " + new FileInfo(path).Length);
    }
}
