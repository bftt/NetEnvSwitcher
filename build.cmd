@echo off

%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe .\src\NetEnvSwitcher.sln /nologo /v:m /p:Configuration="Release" /p:Platform="Any CPU"