@echo off
setlocal
set "DEST=%LOCALAPPDATA%\SymlinkInContextMenu"
echo Reiniciando el Explorador de Windows (se cerraran las ventanas de carpetas)...
taskkill /f /im explorer.exe >nul 2>&1
timeout /t 1 /nobreak >nul
if exist "%DEST%\SymlinkInContextMenu.exe" "%DEST%\SymlinkInContextMenu.exe" --uninstall
rmdir /s /q "%DEST%" >nul 2>&1
start explorer.exe
echo.
echo Desinstalado correctamente / Uninstalled successfully.
timeout /t 3 >nul
