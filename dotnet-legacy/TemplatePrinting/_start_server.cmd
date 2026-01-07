@echo off
set PORT=%1
if "%PORT%"=="" set PORT=444
echo Starting TemplatePrinting on port %PORT%...
dotnet TemplatePrinting.dll --urls=https://0.0.0.0:%PORT%
