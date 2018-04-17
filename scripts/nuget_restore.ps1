$currentDir = (Get-Item -Path ".\" -Verbose).FullName

$report = "Working in directory: " + $currentDir
echo $report

$slnFiles = [System.IO.Directory]::GetFiles($currentDir, "*.sln", [System.IO.SearchOption]::AllDirectories)

for ($i = 0; $i -lt $slnFiles.Length; ++$i) {
    $slnFile = $slnFiles[$i]
    $fileInfo = New-Object -TypeName System.IO.FileInfo -ArgumentList $slnFile
    $fullName = $fileInfo.FullName
    $report = [string]::Format("Restoring {0} ... ({1}/{2})", $fullName, ($i + 1), $slnFiles.Length)
    echo $report
    $nugetCmd = [string]::Format("nuget restore `"{0}`"", $fullName)
    $script = [ScriptBlock]::Create($nugetCmd)

    Invoke-Command -ScriptBlock $script
}

echo "Done."
