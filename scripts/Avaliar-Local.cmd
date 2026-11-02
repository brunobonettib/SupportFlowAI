@echo off
setlocal
call "%~dp0Local-Dotnet.cmd"
set "Analysis__Provider=Ollama"
if not defined Ollama__Model set "Ollama__Model=gemma3:4b"
set "ASPNETCORE_ENVIRONMENT=Development"
set "DOTNET_ENVIRONMENT=Development"
set "EVALUATION_SET=%~1"
if "%EVALUATION_SET%"=="" set "EVALUATION_SET=original"
if not "%EVALUATION_SET%"=="original" if not "%EVALUATION_SET%"=="holdout" exit /b 2
echo Avaliacao REAL local: aquecimento + 15 mensagens ficticias, sem API paga.
dotnet run --project SupportFlowAI.csproj --no-build --no-launch-profile -- --evaluate --allow-local-evaluation --warm-up --evaluation-set=%EVALUATION_SET%
exit /b %ERRORLEVEL%
