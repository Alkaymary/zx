$dir = "c:\xampp\htdocs\all\k\MyApi - Copy\wwwroot"
$files = Get-ChildItem -Path $dir -Filter "*.html"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -notmatch '<link rel="stylesheet" href="/theme.css">') {
        $newContent = $content -replace '</head>', "<link rel=`"stylesheet`" href=`"/theme.css`">`r`n</head>"
        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8
    }
}
Write-Output "Injected theme.css into $($files.Count) HTML files."
