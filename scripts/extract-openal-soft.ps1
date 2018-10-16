param (
    [Parameter(Mandatory=$true)][string]$destination
)

$openAlSoftVersion = '1.19.1'

echo "Downloading OpenAL-Soft ($openAlSoftVersion/Windows)..."
(New-Object Net.WebClient).DownloadFile('http://openal-soft.org/openal-binaries/openal-soft-' + $openAlSoftVersion + '-bin.zip', 'C:\openal-soft.zip')

echo "Extracting OpenAL-Soft..."

$7zCommand = '7z x C:\openal-soft.zip -y -oC:\openal-soft-install\ * -r'
$block = [scriptblock]::Create($7zCommand)

Invoke-Command -ScriptBlock $block

$openAlDllNewName = [System.IO.Path]::Combine($destination, "openal32.dll")
echo "Moving to $openAlDllNewName ..."
$openAlDllOriginalPath = 'C:\openal-soft-install\openal-soft-' + $openAlSoftVersion + '-bin\bin\Win32\soft_oal.dll'
Move-Item -Path $openAlDllOriginalPath -Destination $openAlDllNewName
