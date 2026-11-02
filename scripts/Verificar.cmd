@echo off
setlocal
call "%~dp0Local-Dotnet.cmd"
dotnet restore SupportFlowAI.slnx --verbosity minimal
if errorlevel 1 exit /b 1
dotnet build SupportFlowAI.slnx --no-restore --verbosity minimal
if errorlevel 1 exit /b 1
dotnet test SupportFlowAI.slnx --no-build --no-restore --logger "trx;LogFileName=verification.trx" --results-directory TestResults
if errorlevel 1 exit /b 1
dotnet list SupportFlowAI.slnx package --vulnerable --include-transitive
exit /b %ERRORLEVEL%
