@echo off
setlocal
set "MODE=%~1"
if "%MODE%"=="" set "MODE=--install"
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
set "DEST=%LOCALAPPDATA%\SymlinkInContextMenu"

echo Reiniciando el Explorador de Windows (se cerraran las ventanas de carpetas)...
taskkill /f /im explorer.exe >nul 2>&1
timeout /t 1 /nobreak >nul

if exist "%DEST%" rmdir /s /q "%DEST%" >nul 2>&1
mkdir "%DEST%"
copy /y "%~dp0icon_create.ico" "%DEST%\" >nul
copy /y "%~dp0icon_delete.ico" "%DEST%\" >nul

"%CSC%" /nologo /target:winexe /codepage:65001 /win32icon:"%DEST%\icon_create.ico" /out:"%DEST%\SymlinkInContextMenu.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "%~dp0SymlinkInContextMenu.cs"
if errorlevel 1 (
  start explorer.exe
  echo.
  echo ERROR compilando / Compile error
  pause
  exit /b 1
)
"%DEST%\SymlinkInContextMenu.exe" %MODE%
start explorer.exe
echo.
echo Instalado correctamente / Installed successfully.
timeout /t 3 >nul
