# Run after check-license-free.ps1. Mutations are confined to an ignored temp copy.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$fixture = Join-Path $PSScriptRoot 'LicenseFreeChecks/.test-repo'
$shell = (Get-Process -Id $PID).Path
$checker = Join-Path $PSScriptRoot 'check-license-free.ps1'

function Remove-Fixture {
    $resolved = [IO.Path]::GetFullPath($fixture)
    $expected = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'LicenseFreeChecks/.test-repo'))
    if ($resolved -ne $expected -or -not $resolved.StartsWith([IO.Path]::GetFullPath($root) + [IO.Path]::DirectorySeparatorChar)) {
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

function Assert-Check([string]$Name, [string]$Expected) {
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
        if ($code -ne 1 -or $message -notmatch [regex]::Escape($Expected)) {
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
    Remove-Fixture # Recover a disposable copy left by an interrupted run.
    New-Item -ItemType Directory -Path $fixture | Out-Null
    $files = @(git -C $root ls-files -- Assets)
    if ($LASTEXITCODE -ne 0 -or $files.Count -eq 0) { throw 'Cannot enumerate tracked Assets' }
    foreach ($file in $files) {
        $destination = Join-Path $fixture $file
        New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
        Copy-Item -LiteralPath (Join-Path $root $file) -Destination $destination
    }
    Git-Fixture init --quiet
    Git-Fixture add -- Assets
    Assert-Check 'clean tracked copy' ''

    Add-Content -LiteralPath (Join-Path $fixture $script) -Value "`npublic class Broken {"
    Assert-Check 'C# syntax error' 'error CS'
    Copy-Item -LiteralPath (Join-Path $root $script) -Destination (Join-Path $fixture $script)

    $languageCases = @(
        @{ Name = 'C# 10 file-scoped namespace'; Source = 'namespace Future; public class NewSyntax {}' },
        @{ Name = 'C# 11 raw string literal'; Source = 'public class NewSyntax { string Text = """raw"""; }' },
        @{ Name = 'C# 12 primary constructor'; Source = 'public class NewSyntax(int value) {}' }
    )
    foreach ($case in $languageCases) {
        [IO.File]::WriteAllText((Join-Path $fixture $script), $case.Source)
        Assert-Check $case.Name 'error CS8773'
    }
    [IO.File]::WriteAllText((Join-Path $fixture $script), 'public record Valid { public int Value { get; init; } public static Valid Make() => new(); }')
    Assert-Check 'valid C# 9 record, init and target-typed new' ''
    [IO.File]::WriteAllText((Join-Path $fixture $script), 'public class SemanticOnly : UnresolvedUnityType {}')
    Assert-Check 'unresolved types remain outside the syntax gate' ''
    Copy-Item -LiteralPath (Join-Path $root $script) -Destination (Join-Path $fixture $script)

    $ignoredPaths = @(
        'Assets/PlaceholderOnly/.gitkeep',
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

    Remove-Item -LiteralPath (Join-Path $fixture $script)
    Assert-Check 'tracked file missing on disk' 'Tracked file is missing from the working tree'
    Copy-Item -LiteralPath (Join-Path $root $script) -Destination (Join-Path $fixture $script)

    Assert-Check 'restored clean copy' ''
} finally {
    Remove-Fixture
}
