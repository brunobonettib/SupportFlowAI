@echo off
setlocal
call "%~dp0Local-Dotnet.cmd"
set "Analysis__Provider=Ollama"
if not defined Ollama__Model set "Ollama__Model=gemma3:4b"
set "ASPNETCORE_ENVIRONMENT=Development"
set "DOTNET_ENVIRONMENT=Development"
dotnet run --project SupportFlowAI.csproj --no-build --no-launch-profile --urls http://localhost:5143
exit /b %ERRORLEVEL%
