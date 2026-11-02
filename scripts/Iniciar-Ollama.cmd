@echo off
setlocal
cd /d "%~dp0.."
if not exist ".runtime\ollama\ollama.exe" (
  echo Ollama portatil ausente. Consulte README.md.
  exit /b 1
)
if not exist ".runtime\home" mkdir ".runtime\home"
if not exist ".runtime\tmp" mkdir ".runtime\tmp"
if not exist ".runtime\localappdata" mkdir ".runtime\localappdata"
set "USERPROFILE=%CD%\.runtime\home"
set "LOCALAPPDATA=%CD%\.runtime\localappdata"
set "TEMP=%CD%\.runtime\tmp"
set "TMP=%CD%\.runtime\tmp"
set "OLLAMA_HOST=127.0.0.1:11434"
set "OLLAMA_MODELS=%CD%\.runtime\models"
set "OLLAMA_NO_CLOUD=1"
set "OLLAMA_DEBUG=false"
set "OLLAMA_DEBUG_LOG_REQUESTS=false"
set "OLLAMA_NOPRUNE=1"
set "OLLAMA_NUM_PARALLEL=1"
echo Ollama local, nuvem desativada. Ctrl+C para encerrar.
".runtime\ollama\ollama.exe" serve
exit /b %ERRORLEVEL%
