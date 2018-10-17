param (
    [Parameter(Mandatory=$true)][string]$destination32,
    [Parameter(Mandatory=$true)][string]$destination64
)

$openAlSoftVersion = '1.19.1'

echo "Downloading OpenAL-Soft ($openAlSoftVersion/Windows)..."
(New-Object Net.WebClient).DownloadFile('http://openal-soft.org/openal-binaries/openal-soft-' + $openAlSoftVersion + '-bin.zip', 'C:\openal-soft.zip')

echo "Extracting OpenAL-Soft..."

$7zCommand = '7z x C:\openal-soft.zip -y -oC:\openal-soft-install\ * -r'
$block = [scriptblock]::Create($7zCommand)

Invoke-Command -ScriptBlock $block

# 32-bit
$openAlDllNewName = [System.IO.Path]::Combine($destination32, "openal32.dll")
echo "Moving 32-bit DLL to $openAlDllNewName ..."
$openAlDllOriginalPath = 'C:\openal-soft-install\openal-soft-' + $openAlSoftVersion + '-bin\bin\Win32\soft_oal.dll'
Move-Item -Path $openAlDllOriginalPath -Destination $openAlDllNewName

# 64-bit
$openAlDllNewName = [System.IO.Path]::Combine($destination64, "openal32.dll")
echo "Moving 64-bit DLL to $openAlDllNewName ..."
$openAlDllOriginalPath = 'C:\openal-soft-install\openal-soft-' + $openAlSoftVersion + '-bin\bin\Win64\soft_oal.dll'
Move-Item -Path $openAlDllOriginalPath -Destination $openAlDllNewName
