$configFile = "$env:APPLICATION_PATH\JsPlc.Ssc.PetrolPricing.WinService.exe.config"
[xml] $xmlfile = Get-Content $configFile
write-host "Transforming the config file" $configFile
foreach ($item in $xmlfile.configuration.appSettings.add){
  if($item.key -eq "ServicesBaseUrl"){
    write-host "Updating the app settings for " $item.key " to " $env:ServicesBaseUrl
    $item.value = $env:ServicesBaseUrl
  } 
}
$xmlfile.Save( (Resolve-Path($configFile)))
write-host "Transformation completed for App.config."

#Delete the Windows Service log if it exists and then recreate it

remove-eventlog "PetrolPricingWinService"
new-eventlog -source "PetrolPricingWinService" -logname "PetrolPricingWinService"

