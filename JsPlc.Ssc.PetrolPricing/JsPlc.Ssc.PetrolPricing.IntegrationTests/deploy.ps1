$configFile = "$env:APPLICATION_PATH\app.config"
[xml] $xmlfile = Get-Content $configFile
write-host "Transforming the config file" $configFile
foreach ($item in $xmlfile.configuration.appSettings){
  if($item.add.key -eq "ServicesBaseUrl"){
    write-host "Updating the connection string to " $env:ServicesBaseUrlAppConfig
    $item.add.value = $env:ServicesBaseUrlAppConfig
  }
}
$xmlfile.Save( (Resolve-Path($configFile)))
write-host "Transformation completed."
