@echo off
rem Ambiente limitado ao processo pelo setlocal do chamador.
cd /d "%~dp0.."
set "DOTNET_CLI_HOME=%CD%\.dotnet-home"
set "NUGET_PACKAGES=%CD%\.nuget\packages"
set "NUGET_HTTP_CACHE_PATH=%CD%\.nuget\http-cache"
set "NUGET_PLUGINS_CACHE_PATH=%CD%\.nuget\plugins-cache"
set "DOTNET_CLI_TELEMETRY_OPTOUT=1"
set "DOTNET_GENERATE_ASPNET_CERTIFICATE=false"
set "DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false"
set "DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE=true"
exit /b 0
