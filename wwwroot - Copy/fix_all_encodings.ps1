$dir = "c:\xampp\htdocs\all\k\MyApi - Copy\wwwroot"
$files = Get-ChildItem -Path $dir -Filter "*.html"

foreach ($file in $files) {
    if ($file.Name -eq "index.html") { continue }
    
    $text = [IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    $ansiEncoding = [System.Text.Encoding]::GetEncoding(1252)
    $originalBytes = $ansiEncoding.GetBytes($text)
    $restoredText = [System.Text.Encoding]::UTF8.GetString($originalBytes)
    
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [IO.File]::WriteAllText($file.FullName, $restoredText, $utf8NoBom)
}
Write-Output "Restored encoding for $($files.Count - 1) HTML files."
