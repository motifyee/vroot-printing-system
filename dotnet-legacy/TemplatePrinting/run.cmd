@echo off

set PORT=444
set FOLDER=publishN

if not exist _start_server.cmd (
    cd %FOLDER%
)
echo Calling _start_server.cmd with port %PORT%...
call _start_server.cmd %PORT%
pause
