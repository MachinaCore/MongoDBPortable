@echo off

SET APPNAME=PostgreSQLPortable
SET NSISLAUNCHER=0
SET SEVENZIP=0
SET PORTABLEAPPSINSTALLER=1
SET BASE_DIR=%CD%

REM ---------------------------------------------------------------------
echo Get Version
REM ---------------------------------------------------------------------
set APPINFOINI=App\AppInfo\appinfo.ini
set APPINFOSECTION=Version
set APPINFOKEY=DisplayVersion

for /f "delims=:" %%a in ('findstr /binc:"[%APPINFOSECTION%]" "%APPINFOINI%"') do (
  for /f "tokens=1* delims==" %%b in ('more +%%a^<"%APPINFOINI%"') do (
    set "APPINFOKEY=%%b"
    set "APPINFOVERSION=%%c"
    setlocal enabledelayedexpansion
    if "!APPINFOKEY:~,1!"=="[" (endlocal&goto notFound)
    if /i "!APPINFOKEY!"=="%APPINFOKEY%" (endlocal&goto found)
    endlocal
  )
) 

:notFound
set APPINFOVERSION=0.0.0.0

:found
echo %APPINFOVERSION%

REM ---------------------------------------------------------------------
echo Prepare Release
REM ---------------------------------------------------------------------
rmdir /s /q Release
mkdir Release

robocopy App Release\App /s /e
robocopy Other\Help Release\Other\Help /s /e

copy PostgreSQLPortable.exe Release\PostgreSQLPortable.exe
copy help.html Release\help.html

REM ---------------------------------------------------------------------
echo 7zip Archive
REM ---------------------------------------------------------------------
if %SEVENZIP%==1 (
del "%CD%\%APPNAME%_%APPINFOVERSION%.7z"
..\7-ZipPortable\App\7-Zip\7z.exe a -r -t7z -mx=9 %APPNAME%_%APPINFOVERSION%.7z .\Release\*
)

REM ---------------------------------------------------------------------
echo Create PortableApps Installer
REM ---------------------------------------------------------------------
if %PORTABLEAPPSINSTALLER%==1 (
..\GathSystems.comAppInstaller\PortableApps.comInstaller.exe "%CD%\Release"
)

rmdir /s /q Release

copy %APPNAME%_%APPINFOVERSION%.paf.exe ..\..\AppRelease\%APPNAME%_%APPINFOVERSION%.paf.exe
copy App\AppInfo\appinfo.ini ..\..\AppRelease\%APPNAME%.ini

REM ---------------------------------------------------------------------
echo Ready
REM ---------------------------------------------------------------------
pause
