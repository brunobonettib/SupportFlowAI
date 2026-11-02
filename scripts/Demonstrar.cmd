@echo off
setlocal
cd /d "%~dp0.."
echo Requer API iniciada por scripts\Iniciar-Api-Local.cmd. Uma mensagem ficticia.
curl.exe --silent --show-error --fail-with-body --max-time 130 --header "Content-Type: application/json" --data-binary "@evaluation/demo-request.json" --write-out "\nHTTP: %%{http_code} | segundos: %%{time_total}\n" http://localhost:5143/api/tickets/analyze
exit /b %ERRORLEVEL%
