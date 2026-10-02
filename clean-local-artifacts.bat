@echo off
setlocal

rem Run from the repository root. Removes local build, test, and Visual Studio artifacts.
if not exist "NQueen.slnx" (
    echo This script must be run from the repository root.
    exit /b 1
)

echo Cleaning bin and obj folders...
for /d /r %%D in (bin,obj) do (
    if exist "%%D" (
        echo Removing "%%D"
        rmdir /s /q "%%D" 2>nul
        if exist "%%D" echo Could not remove "%%D". Close apps that may be using it and retry.
    )
)

echo Cleaning root Visual Studio state...
if exist ".vs" (
    echo Removing ".vs"
    rmdir /s /q ".vs" 2>nul
    if exist ".vs" echo Could not remove ".vs". Close Visual Studio and retry.
)

echo Cleaning test results...
if exist "TestResults" (
    echo Removing "TestResults"
    rmdir /s /q "TestResults" 2>nul
    if exist "TestResults" echo Could not remove "TestResults".
)

echo Cleanup complete.
echo %cmdcmdline% | find /i "/c" >nul && pause
endlocal
