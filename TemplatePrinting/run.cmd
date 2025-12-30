@echo off
if not exist _start_server.cmd (
    cd publish
)
echo Calling _start_server.cmd with port 444...
call _start_server.cmd 444
pause
