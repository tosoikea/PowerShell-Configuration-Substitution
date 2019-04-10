# PSake makes variables declared here available in other scriptblocks
# Init some things
Properties {
    # Find the build folder based on build system
    $ProjectRoot = $ENV:BHProjectPath
    if(-not $ProjectRoot)
    {
        $ProjectRoot = $PSScriptRoot
    }

    $Timestamp = Get-date -uformat "%Y%m%d-%H%M%S"
    $PSVersion = $PSVersionTable.PSVersion.Major
    $TestFile = "TestResults_PS$PSVersion`_$TimeStamp.xml"
    $lines = '----------------------------------------------------------------------'

    $Verbose = @{}
    if($ENV:BHCommitMessage -match "!verbose")
    {
        $Verbose = @{Verbose = $True}
    }
}

Task Default -Depends Deploy

Task Init {
    $lines
    Set-Location $ProjectRoot
    "Build System Details:"
    Get-Item ENV:BH* | Format-List
    "`n"
}

Task Test -Depends Init  {
    $lines
    "`n`tSTATUS: Testing with PowerShell $PSVersion"

    # Gather test results. Store them in a variable and file
    $TestResults = Invoke-Pester -Path $ProjectRoot\Tests -PassThru -OutputFormat NUnitXml -OutputFile "$ProjectRoot\$TestFile"

    # In Appveyor?  Upload our tests! #Abstract this into a function?
    If($ENV:BHBuildSystem -eq 'AppVeyor')
    {
        (New-Object 'System.Net.WebClient').UploadFile(
            "https://ci.appveyor.com/api/testresults/nunit/$($env:APPVEYOR_JOB_ID)",
            "$ProjectRoot\$TestFile" )
    }

    Remove-Item "$ProjectRoot\$TestFile" -Force -ErrorAction SilentlyContinue

    # Failed tests?
    # Need to tell psake or it will proceed to the deployment. Danger!
    if($TestResults.FailedCount -gt 0)
    {
        Write-Error "Failed '$($TestResults.FailedCount)' tests, build failed"
    }
    "`n"
}

Task Build -Depends Test {
    $lines
    
    # Load the module, read the exported functions, update the psd1 FunctionsToExport
    Set-ModuleFunctions

    # Bump the module version
    Update-Metadata -Path $env:BHPSModuleManifest
}

Task BuildDocs -Depends Build {
    $lines
    
    [String] $mkPath = [System.IO.Path]::Combine($ProjectRoot, "mkdocs.yml")
    [String] $docDirectory = [System.IO.Path]::Combine($ProjectRoot, "docs")
    [String] $funcDocPath = [System.IO.Path]::Combine($docDirectory, "functions")

    [hashtable] $moduleData = Import-PowerShellDataFile -Path $env:BHPSModuleManifest
    
    if (!$moduleData) {
        throw [System.ArgumentNullException]::new("Missing module manifest data at : " + $env:BHPSModuleManifest)
    }

    [String] $moduleName = $moduleData.RootModule -replace "\.psm1"
    Import-Module $env:BHPSModuleManifest -Force

    $generatedMarkdown = New-MarkdownHelp -Module $moduleName -OutputFolder $funcDocPath -Force
    #Display creation result
    "Created Markdown files :`n"
    $generatedMarkdown.Name


    [hashtable] $docData = @{ }

    if (Get-Item -Path $mkPath -ErrorAction SilentlyContinue) {
        $docData = ConvertFrom-Yaml -Yaml (Get-Content -Pat $mkPath -Raw)
    }

    if(!$docData -or $docData.Count -eq 0){
        $docData = @{
            "site_name" = ([String](Get-ProjectName))
            "edit_uri"  = "edit/master/docs/"
            "theme"    = "readthedocs"
        }
    }

    $docData.Item("site_author") = ([String]$moduleData.Author)
    $docData.Item("copyright") = ([String]$moduleData.Copyright)

    if (!$docData.ContainsKey("pages")) {
        $docData.Add("pages", @(
                @{"Home" = "index.md" }
                @{"Usage" = "usage.md" }
            ))
    }

    [int] $removeIndex = -1
    for ([int] $pI = 0; ($pI -lt ($docData."pages").Count) -and ($removeIndex -eq -1); $pI++){
        $pageEntry = ($docData."pages")[$pI]
        if ($pageEntry.ContainsKey("Functions")){
            $removeIndex = $pI
        }
    }

    if ($removeIndex -ge 0){
        ($docData."pages").RemoveAt($removeIndex)
        "Removed current Functions pages."
    }

    [String[]] $functions = @()
    $functions += Get-ModuleFunction

    if ($functions.Length -eq 0) {
        throw [System.ArgumentNullException]::new("Could not determine any public functions!")
    }

    [Hashtable[]] $functionEntries = @()
    foreach ( $function in $functions ) {
        [String] $markDownFile = [System.IO.Path]::Combine($funcDocPath, "$function.md")
        if (!(Get-Item -Path $markDownFile)) {
            throw [System.ArgumentNullException]::new("Missing : $markDownFile!")
        }
        
        $functionEntries += @{$function = $markDownFile.Substring($docDirectory.Length + 1) }
    }
    $docData."pages" += @{ "Functions" = $functionEntries }

    
    ConvertTo-Yaml -Data $docData -OutFile $mkPath -Force
}

Task Deploy -Depends BuildDocs {
    $lines

    $Params = @{
        Path = $ProjectRoot
        Force = $true
        Recurse = $false # We keep psdeploy artifacts, avoid deploying those : )
    }
    Invoke-PSDeploy @Verbose @Params
}