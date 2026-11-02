# Execute com dot-sourcing: . ./scripts/Use-LocalDotnet.ps1
# Isola os caches e evita alterar o perfil do usuário durante restore/build/test.
$projectRoot = Split-Path -Parent $PSScriptRoot
$env:DOTNET_CLI_HOME = Join-Path $projectRoot '.dotnet-home'
$env:NUGET_PACKAGES = Join-Path $projectRoot '.nuget/packages'
$env:NUGET_HTTP_CACHE_PATH = Join-Path $projectRoot '.nuget/http-cache'
$env:NUGET_PLUGINS_CACHE_PATH = Join-Path $projectRoot '.nuget/plugins-cache'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = 'false'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = 'false'
$env:DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE = 'true'
