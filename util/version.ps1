#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop";
if ($PSVersionTable.PSVersion -lt "6.0") {
    pwsh -f $SCRIPT:MyInvocation.MyCommand.Path
    return
}

function NoE {
    [OutputType([bool])]
    Param  ([parameter(Position = 0)]$string)
    return [string]::IsNullOrEmpty($string)
}

$isPullBuild = $false;
if (!(NoE $env:GITHUB_BASE_REF) {
        $isPullBuild = $true;
    }

    class LocalVersionInfo {
        [string] $PreviousSHA1;
        [string] $NextVersion;
    }

    class GitVersionInfo {
        [string] $SHA1;
        [string] $App;
        [string] $Version;
    }

    function Flatten($a) {
        , @($a | % { $_ })
    }

    function Push-Tags {
        Param  ([string[]]$tags)

        if ($null -eq $tags) {
            return;
        }

        $tags = @($tags)

        foreach ($tag in $tags) {
            Write-Host "##vso[build.addbuildtag]$tag"
        }
    
        if ($tags.Count -gt 0) {
            $gitArgs = Flatten @("push", "origin", $tags)
            & git $gitArgs
            if (!$?) { throw 'Git push command failed' };
        }
    }

    function Set-GitTag {
        [OutputType([string])]
        Param  ([string]$version, [string]$msg, [string]$app)
        $pfx = $version.Split('.')[0..1] -join '.'
        if ($isPullBuild) {
            $pfx += "/PR";
        }
        $id = $app;
        $tag = "vers/$pfx/$id/$version"
        Write-Host "Adding tag $tag with $msg"
        git tag $tag -a -m $msg
        if (!$?) { throw 'Git tag command failed' };
    
        Write-Host "Adding variables for $app";
        $var = "$($id)_version";
        Write-Host "##vso[task.setvariable variable=$var]$version";
        [System.Environment]::SetEnvironmentVariable($var, $version, 0);
        $var = "$($id.ToUpper())_VERSION";
        Write-Host "##vso[task.setvariable variable=$var]$version";
        [System.Environment]::SetEnvironmentVariable($var, $version, 0);
        $var = "osd_$($id)_tag";
        Write-Host "##vso[task.setvariable variable=$var]$tag";
        [System.Environment]::SetEnvironmentVariable($var, $tag, 0);
        $var = "$($id)_changed";
        Write-Host "##vso[task.setvariable variable=$var]true";
        [System.Environment]::SetEnvironmentVariable($var, "true", 0);
        [IO.File]::AppendAllText("$PSScriptRoot/tags", "$tag`n")
        return $tag;
    }

    function Get-RawGitTags() {
        git fetch --prune origin +refs/tags/*:refs/tags/*
        if ($LASTEXITCODE -ne 0) { throw 'unable to lookup / remote old git tags' };
        $tags = Invoke-Expression 'git show-ref --tags -d' -ErrorVariable gitTagsError
        if ($LASTEXITCODE -ne 0 -and ![string]::IsNullOrEmpty($gitTagsError)) { $gitTagsError; throw 'Git show-ref command failed' };
        return $tags;
    }
    function Get-TagVersions {
        [OutputType([GitVersionInfo[]])]
        Param ([String[]]$tags)

        $tags = $tags | ? { $null -ne $_ -and $_.EndsWith('^{}') };
        $results = New-Object Collections.Generic.List[GitVersionInfo];

        foreach ($tag in $tags) {
            $segs = $tag.Split() | where { ![string]::IsNullOrWhiteSpace($_) };
            if ($segs[1].Trim().StartsWith('refs/tags/vers/')) {
                $split = $segs[1].Trim().Split('/');
                $ver = $split[5].Substring(0, $split[5].Length - 3);
                if ($ver.Split('.').Length -ne 4) {
                    Write-Host "Invalid version tag found: $tag"
                    continue;
                }
                $app = $split[4];
                $info = [GitVersionInfo]::new();
                $info.SHA1 = $segs[0];
                $info.App = $app;
                $info.Version = $ver;
                $results.Add($info);
            }
        }
        return $results
    }


    function Get-LatestVersion {
        [OutputType([GitVersionInfo])]
        Param  ([GitVersionInfo[]]$versions, [string]$key, [string]$verPrefix)

        if ($null -eq $versions) {
            return $null;
        }
        $dk = 0;

        # clean this up at some point parse into segs on GitVersionInfo
        $versions = $versions | ? { $segs = $_.Version.Split('.'); return $segs.Length -eq 4 -and [int32]::TryParse($segs[0], [ref]$dk) `
                -and [int32]::TryParse($segs[1], [ref]$dk) `
                -and [int32]::TryParse($segs[2], [ref]$dk) `
                -and [int32]::TryParse($segs[3], [ref]$dk) };

        $autoVers = @(
            $versions `
            | ? { $_.Version.StartsWith($verPrefix) -and $_.App -eq $key } `
            | Sort-Object -Property @{ Expression = { [int]$_.Version.Split('.')[0] }; Ascending = $false; }, `
            @{ Expression = { [int]$_.Version.Split('.')[1] }; Ascending = $false; }, `
            @{ Expression = { [int]$_.Version.Split('.')[2] }; Ascending = $false; }, `
            @{ Expression = { [int]$_.Version.Split('.')[3] }; Ascending = $false; }
        );
        if ($autoVers.Length -eq 0) {
            return $null;
        }
        return $autoVers[0];
    }




    function IsReleaseBuild {
        [OutputType([bool])]
        Param  ()
        $source = $env:BUILD_SOURCEBRANCH;
        if (NoE $source) {
            throw "No source branch in env var.";
        }

        if ($isPullBuild) {
            $target = $env:GITHUB_BASE_REF;
            if (NoE $target) {
                throw "No target branch in env var for pull build.";
            }
            if ($target.StartsWith("refs/heads/releases")) {
                return $true;
            }
            return $false;
        }

        if ($source.StartsWith("refs/heads/releases")) {
            return $true;
        }
    }

    function GetReleaseBaseVersion {
        [OutputType([string])]
        Param  ()
        $source = $env:GITHUB_HEAD_REF;
        if (NoE $source) {
            throw "No source branch in env var.";
        }

        $ver = '';
        if ($isPullBuild) {
            $target = $env:GITHUB_BASE_REF;
            if (NoE $target) {
                throw "No target branch in env var for pull build.";
            }
            if (!$target.StartsWith("refs/heads/releases")) {
                throw "Target branch was not release build.";
            }
            $ver = $target.Split('/')[3];
        }
        elseif ($source.StartsWith("refs/heads/releases")) {
            $ver = $source.Split('/')[3];
        }
        else {
            throw "Build branch was not release build.";
        }
        return $ver;
    }

    function GetTrunkVersionPrefix {
        [OutputType([string])]
        Param  ()

        $current = [IO.File]::ReadAllText($verFilePath);
        return "$current.0";
    }

    function Get-ChangedFiles {
        [OutputType([string[]])]
        Param  ([string]$refSha)

        $files = git diff $refSha HEAD --name-only
        return @($files);
    }

    function Normalize {
        [OutputType([string])]
        Param  ([string]$path)
        $path = [string]$path.Replace('\', '/')
        if ($path[0] -ne '/') {
            return "/$path";
        }
        return $path;
    }



    function RunVersioning() {
        [String[]]$existingTags = Get-RawGitTags;
        $versions = Get-TagVersions($existingTags)
        $app = "repo";
        $ver = "";
        $msg = "";
    
        if (IsReleaseBuild) {
            [IO.File]::WriteAllText("$PSScriptRoot/RELEASE_BUILD", 'true')
            # release versioning, new release or hotfix
            [string]$base = GetReleaseBaseVersion;
            Write-Host "Release base version is $base";
            $segs = $base.Split('.');
            $prefix = $segs[0..2] -join '.';
            $lastAuto = Get-LatestVersion -versions $versions -key $app -verPrefix $prefix;
            if ($Null -eq $lastAuto) {
                Write-Host "On release branch with no previous version, using branch version."
                $ver = $base;
                $msg = "New release $ver for $app";
            }
            else {
                Write-Host "Detected release hotfix, bumping minor."
                $segs = $lastAuto.Version.Split('.');
                Write-Host "Last version was $($lastAuto.Version)";
                $nxt = [int]$segs[3] + 1;
                $pfx = $segs[0..2] -join '.'
                $ver = "$pfx.$nxt";
                $msg = "New hotfix $ver for $app";
            }
        }
        else {
            [IO.File]::Delete("$PSScriptRoot/RELEASE_BUILD")
            # versioning, on trunk
            # since these are CI builds add 1
            [string]$current = GetTrunkVersionPrefix;
            Write-Host "Trunk version prefix is $current";
        
            $lastAuto = Get-LatestVersion -versions $versions -key $app -verPrefix $current;
            if ($null -eq $lastAuto) {
                Write-Host "No previous verions for $app, creating new."
                $ver = "$current.0.0";
                $msg = "New version $ver for $app";
            }
            else {
                # switch to using patches for trunk CI
                Write-Host "Previous verions for $app is $($lastAuto.Version), creating new."
                $segs = $lastAuto.Version.Split('.');
                $nxt = [int]$segs[3] + 1;
                $pfx = $segs[0..2] -join '.'
                $ver = "$pfx.$nxt";
                $msg = "New version $ver for $app";
            }
        }
    
        Write-Host $msg;
        $tag = Set-GitTag -version $ver -app $app -msg $msg;

        $iv = $ver;
        if ($isPullBuild) {
        
            $prId = $env:SYSTEM_PULLREQUEST_PULLREQUESTID;
            $shaShort = git rev-parse --short HEAD
            $shaShort = $shaShort.toUpper();
            if (!$?) {
                throw "git sha lookup failed;"
            }
            $iv = "$ver-$prId" + "PR-$shaShort";
            if ($null -ne $env:USE_BRANCH_FOR_PULL) {
                $bRef = $env:SYSTEM_PULLREQUEST_SOURCEBRANCH
                $branch = $bRef.Split("/") | Select-Object -Skip 2 | Join-String -Separator '/'
                $iv = "PR-$branch-$shaShort".Replace('/', '-').Replace('.', '-').ToLower() -replace '[^0-9a-z\-]', '';
            }
            $typeTag = "pr-build";
            Write-Host "##vso[build.addbuildtag]$typeTag"
        }
        else {
            $typeTag = "ci-build";
            if (IsReleaseBuild) {
                $typeTag = "release-build";
            }
            Write-Host "##vso[build.addbuildtag]$typeTag"
        }
        Write-Host "##vso[build.updatebuildnumber]$iv"
    
        [IO.File]::WriteAllText("$PSScriptRoot/THIS_VERSION", $iv);
    }
