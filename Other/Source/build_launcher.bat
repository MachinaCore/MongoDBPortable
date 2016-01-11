SET BASE_DIR=%CD%
cd..
cd..

C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc /win32icon:App/AppInfo/appicon.ico /reference:SharpConfig.dll /lib:Other\Source /out:PostgreSQLPortable.exe "%BASE_DIR%\PostgreSQLPortable.cs"

COPY PostgreSQLPortable.exe Temp.Exe
"%BASE_DIR%\ILMerge.exe" /out:"PostgreSQLPortable.exe" "Temp.Exe" "%BASE_DIR%\SharpConfig.dll"
DEL Temp.Exe
DEL PostgreSQLPortable.pdb

PostgreSQLPortable.exe
pause