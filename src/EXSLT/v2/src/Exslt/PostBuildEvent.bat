@echo off
@echo ########### Setting environment variables
call "D:\Program Files\Microsoft Visual Studio .NET 2003\Common7\IDE\..\Tools\vsvars32.bat"
nmake
if errorlevel 1 goto CSharpReportError
goto CSharpEnd
:CSharpReportError
echo Project error: A tool returned an error code from the build event
exit 1
:CSharpEnd