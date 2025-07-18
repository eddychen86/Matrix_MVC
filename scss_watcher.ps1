$watcher = New-Object System.IO.FileSystemWatcher 
$watcher.Path = "wwwroot\css" 
$watcher.Filter = "components.css" 
$watcher.IncludeSubdirectories = $false 
$watcher.EnableRaisingEvents = $true 

$action = { 
    $path = $Event.SourceEventArgs.FullPath 
    $changeType = $Event.SourceEventArgs.ChangeType 
    $timeStamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss" 
    Write-Host "[$timeStamp] components.css is change: $changeType - $path" 
    "SCSS_CHANGED" | Out-File -FilePath "scss_change_signal.tmp" -Encoding ASCII 
} 

Register-ObjectEvent -InputObject $watcher -EventName "Changed" -Action $action 

Write-Host "Start to watching components.css, watch location: wwwroot\css" 
Write-Host "Press Ctrl+C to stop watching..." 

try { 
    while ($true) { 
        Start-Sleep -Seconds 1 
    } 
} finally { 
    $watcher.EnableRaisingEvents = $false 
    $watcher.Dispose() 
    Write-Host "components.css watcher stopped." 
} 
