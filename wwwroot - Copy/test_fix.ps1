$file = "c:\xampp\htdocs\all\k\MyApi - Copy\wwwroot\index.html"

# Read the double-encoded UTF-8 file
$text = [IO.File]::ReadAllText($file, [System.Text.Encoding]::UTF8)

# Convert the string back to the ANSI bytes that were incorrectly read originally
$ansiEncoding = [System.Text.Encoding]::GetEncoding(1252)
$originalBytes = $ansiEncoding.GetBytes($text)

# Decode these original bytes as the true UTF-8 string
$restoredText = [System.Text.Encoding]::UTF8.GetString($originalBytes)

# Write it back properly
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[IO.File]::WriteAllText($file, $restoredText, $utf8NoBom)
Write-Output "Success"
