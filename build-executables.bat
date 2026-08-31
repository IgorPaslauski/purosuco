@echo off
echo Compilando executaveis do PuroSuco (CLI e LSP)...

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-executables.ps1"
