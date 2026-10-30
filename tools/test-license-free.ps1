# Run after check-license-free.ps1. Mutations are confined to a disposable temp copy.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$fixtureName = 'wam-license-free-' + [guid]::NewGuid().ToString('N')
$fixture = Join-Path $tempRoot $fixtureName
$shell = (Get-Process -Id $PID).Path
$checker = Join-Path $PSScriptRoot 'check-license-free.ps1'

function Remove-Fixture {
    $resolved = [IO.Path]::GetFullPath($fixture)
    $expected = [IO.Path]::GetFullPath((Join-Path $tempRoot $fixtureName))
    if ($resolved -ne $expected -or (Split-Path $resolved -Parent) -ne $tempRoot.TrimEnd([IO.Path]::DirectorySeparatorChar)) {
        throw 'Refusing to remove an unexpected fixture path'
    }
    if (Test-Path -LiteralPath $resolved) {
        if ((Get-Item -Force -LiteralPath $resolved).Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw 'Refusing to remove a linked fixture path'
        }
        Remove-Item -LiteralPath $resolved -Recurse -Force
    }
}

function Git-Fixture {
    & git -C $fixture @args
    if ($LASTEXITCODE -ne 0) { throw 'Fixture Git command failed' }
}

function Assert-Check([string]$Name, [string]$Expected, [int]$ExpectedExitCode = 1, [switch]$ExactMessage) {
    # Redirect stderr as well; a failing checker is the expected result in negative cases.
    $savedPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue' # Windows PowerShell wraps native stderr in ErrorRecord.
        $log = & $shell -NoProfile -File $checker -RepositoryRoot $fixture 2>&1
        $code = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $savedPreference
    }
    $message = $log -join "`n"
    if ($Expected) {
        $pattern = [regex]::Escape($Expected)
        if ($ExactMessage) { $pattern = '(?m)^' + $pattern + '\r?$' }
        if ($code -ne $ExpectedExitCode -or $message -notmatch $pattern) {
            throw "$Name failed (exit $code):`n$message"
        }
    } elseif ($code -ne 0) {
        throw "$Name failed (exit $code):`n$message"
    }
    Write-Output "PASS: $Name (exit $code)"
}

$script = 'Assets/Scripts/Utilities/GameConstants.cs'
$meta = "$script.meta"
$folderMeta = 'Assets/Scripts.meta'
$orphan = 'Assets/DeliberatelyOrphaned.txt.meta'
try {
    New-Item -ItemType Directory -Path $fixture | Out-Null
    $files = @(git -C $root ls-files -- Assets)
    if ($LASTEXITCODE -ne 0 -or $files.Count -eq 0) { throw 'Cannot enumerate tracked Assets' }
    foreach ($file in $files) {
        $destination = Join-Path $fixture $file
        New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $root $file) -Destination $destination
    }
    Git-Fixture init --quiet
    # Avoid machine-specific line-ending warnings becoming errors in Windows PowerShell.
    Git-Fixture config core.autocrlf false
    Git-Fixture add -- Assets
    Assert-Check 'clean tracked copy' '0 error(s).' 0

    Remove-Item -LiteralPath (Join-Path $fixture $script)
    Assert-Check 'tracked file deleted from working tree' "Tracked file is missing from the working tree: $script" -ExactMessage
    Copy-Item -LiteralPath (Join-Path $root $script) -Destination (Join-Path $fixture $script)

    # Git ignores do not remove existing index entries or exempt tracked files.
    $gitignore = Join-Path $fixture '.gitignore'
    [IO.File]::WriteAllText($gitignore, "/$script`n")
    Git-Fixture add -- .gitignore
    Git-Fixture check-ignore --no-index --quiet -- $script
    Git-Fixture ls-files --error-unmatch -- $script | Out-Null
    Assert-Check 'tracked + gitignored file present' '0 error(s).' 0
    Remove-Item -LiteralPath (Join-Path $fixture $script)
    Assert-Check 'tracked + gitignored file deleted from working tree' "Tracked file is missing from the working tree: $script" -ExactMessage
    Copy-Item -LiteralPath (Join-Path $root $script) -Destination (Join-Path $fixture $script)
    Remove-Item -LiteralPath $gitignore
    Git-Fixture add -A -- .gitignore

    Add-Content -LiteralPath (Join-Path $fixture $script) -Value "`npublic class Broken {"
    Assert-Check 'C# syntax error' 'error CS'
    Copy-Item -LiteralPath (Join-Path $root $script) -Destination (Join-Path $fixture $script)

    $languageCases = @(
        @{ Name = 'C# 10 file-scoped namespace'; Diagnostic = 'CS8773'; Source = 'namespace Future; public class NewSyntax {}' },
        @{ Name = 'C# 11 raw string literal'; Diagnostic = 'CS8773'; Source = 'public class NewSyntax { string Text = """raw"""; }' },
        @{ Name = 'C# 12 primary constructor'; Diagnostic = 'CS8773'; Source = 'public class NewSyntax(int value) {}' },
        @{ Name = 'C# 11 unsigned right shift (binding)'; Diagnostic = 'CS8773'; Source = 'public class NewSyntax { public int Shift(int a) => a >>> 1; }' },
        @{ Name = 'C# 13 params collection (binding)'; Diagnostic = 'CS8773'; Source = 'public class NewSyntax { public void M(params System.ReadOnlySpan<int> values) {} }' },
        @{ Name = 'C# 11 span constant pattern (binding)'; Diagnostic = 'CS8773'; Source = 'public class NewSyntax { public bool Match(System.ReadOnlySpan<char> text) => text is "hello"; }' },
        @{ Name = 'C# 13 index initializer (binding)'; Diagnostic = 'CS8773'; Source = 'public class NewSyntax { public object Make() => new System.Collections.Generic.List<int> { [^1] = 1 }; }' },
        @{ Name = 'C# 10 interpolated string handler (binding)'; Diagnostic = 'CS8773'; Source = 'public class NewSyntax { static void Log(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler handler) {} public void M() => Log($"value {1}"); }' },
        @{ Name = 'C# 11 static abstract interface member'; Diagnostic = 'CS8703'; Source = 'public interface NewSyntax { static abstract int M(); }' },
        @{ Name = 'C# 14 null-conditional assignment'; Diagnostic = 'CS8652'; Source = 'public class NewSyntax { public int X; public void M(NewSyntax a) { a?.X = 1; } }' },
        @{ Name = 'C# 11 newline in interpolation hole'; Diagnostic = 'CS8967'; Source = 'public class NewSyntax { string Text = $"{
            1}"; }' }
    )
    foreach ($case in $languageCases) {
        [IO.File]::WriteAllText((Join-Path $fixture $script), $case.Source)
        Assert-Check $case.Name "error $($case.Diagnostic)"
    }
    [IO.File]::WriteAllText((Join-Path $fixture $script), 'public record Valid { public int Value { get; init; } public static Valid Make() => new(); }')
    Assert-Check 'valid C# 9 record, init and target-typed new' ''
    [IO.File]::WriteAllText((Join-Path $fixture $script), 'public class SemanticOnly : UnresolvedUnityType {}')
    Assert-Check 'unresolved types remain outside the syntax gate' ''
    Copy-Item -LiteralPath (Join-Path $root $script) -Destination (Join-Path $fixture $script)

    $folderCases = @(
        @{ Name = '.gitkeep-only folder with meta'; Folder = 'Assets/Placeholder'; File = 'Assets/Placeholder/.gitkeep' },
        @{ Name = 'Samples~-only folder with meta'; Folder = 'Assets/Pkg'; File = 'Assets/Pkg/Samples~/x.cs' }
    )
    foreach ($case in $folderCases) {
        $destination = Join-Path $fixture $case.File
        New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
        [IO.File]::WriteAllText($destination, 'deliberately invalid C# with no meta')
        $folderMetaPath = "$($case.Folder).meta"
        [IO.File]::WriteAllText((Join-Path $fixture $folderMetaPath), "fileFormatVersion: 2`nguid: $([guid]::NewGuid().ToString('N'))`nfolderAsset: yes`n")
        Git-Fixture add --force -- $case.File $folderMetaPath
        Assert-Check $case.Name ''
    }

    $ignoredPaths = @(
        'Assets/.gitkeep',
        'Assets/.DS_Store',
        'Assets/backup.txt~',
        'Assets/Foo~/Nested/Broken.cs',
        'Assets/.hidden/Nested/Broken.cs',
        'Assets/scratch.tmp',
        'Assets/cvs/Nested/Broken.cs'
    )
    foreach ($path in $ignoredPaths) {
        $destination = Join-Path $fixture $path
        New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
        [IO.File]::WriteAllText($destination, 'deliberately invalid C# with no meta')
        Git-Fixture add --force -- $path
        Assert-Check "Unity-ignored $path" ''
        Remove-Item -LiteralPath $destination
        Assert-Check "Unity-ignored tracked file deleted: $path" "Tracked file is missing from the working tree: $path" -ExactMessage
        [IO.File]::WriteAllText($destination, 'deliberately invalid C# with no meta')
    }

    Remove-Item -LiteralPath (Join-Path $fixture $meta)
    Git-Fixture add -A -- Assets
    Assert-Check 'missing tracked file .meta' "Missing tracked .meta: $meta"
    Copy-Item -LiteralPath (Join-Path $root $meta) -Destination (Join-Path $fixture $meta)
    Git-Fixture add -A -- Assets

    Remove-Item -LiteralPath (Join-Path $fixture $folderMeta)
    Git-Fixture add -A -- Assets
    Assert-Check 'missing tracked folder .meta' "Missing tracked .meta: $folderMeta"
    Copy-Item -LiteralPath (Join-Path $root $folderMeta) -Destination (Join-Path $fixture $folderMeta)
    Git-Fixture add -A -- Assets

    Copy-Item -LiteralPath (Join-Path $fixture $meta) -Destination (Join-Path $fixture $orphan)
    Git-Fixture add -A -- Assets
    Assert-Check 'orphan .meta' "Orphan .meta: $orphan"
    Remove-Item -LiteralPath (Join-Path $fixture $orphan)
    Git-Fixture add -A -- Assets

    $otherMeta = Join-Path $fixture 'Assets/Scripts/Utilities/CameraShake.cs.meta'
    $original = [IO.File]::ReadAllText($otherMeta)
    $guid = [regex]::Match([IO.File]::ReadAllText((Join-Path $fixture $meta)), 'guid: ([a-f0-9]{32})').Groups[1].Value
    [IO.File]::WriteAllText($otherMeta, [regex]::Replace($original, 'guid: [a-f0-9]{32}', "guid: $($guid.ToUpperInvariant())"))
    Assert-Check 'duplicate GUID (case insensitive)' 'Duplicate GUID'
    [IO.File]::WriteAllText($otherMeta, $original)

    [IO.File]::WriteAllText($otherMeta, [regex]::Replace($original, 'guid: [a-f0-9]{32}', 'guid: invalid'))
    Assert-Check 'malformed GUID' 'Expected exactly one 32-hex GUID'
    [IO.File]::WriteAllText($otherMeta, $original)

    Assert-Check 'restored clean copy' ''
} finally {
    Remove-Fixture
}
