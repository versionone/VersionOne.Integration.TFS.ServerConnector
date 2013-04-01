@ECHO OFF
SETLOCAL
SET PROJECT="VersionOne.ServerConnector.csproj"
FOR /D %%D IN (%SYSTEMROOT%\Microsoft.NET\Framework\*) DO SET MSBUILD="%%D\MSBuild.exe"
REM SET MSBUILD=
SET NUGET="%CD%\.nuget\NuGet.exe"
SET NUGET_PKG="%CD%\packages"
SET NUGET_SRC="http://packages.nuget.org/api/v2/;http://www.myget.org/F/versionone/"
SET BUILD_TOOLS=%1

ECHO.
ECHO Beginning build for %PROJECT%
ECHO Using msbuild at %MSBUILD%
ECHO Using nuget at %NUGET%
ECHO Using nuget package at %NUGET_PKG%
ECHO Using nuget sources %NUGET_SRC%
ECHO Using V1BuildTools at %BUILD_TOOLS%
ECHO.

mkdir %NUGET_PKG%

%MSBUILD% %PROJECT% /t:RestorePackages ^
/p:NuGetExePath=%NUGET% ^
/p:PackageSources=%NUGET_SRC% ^
/p:RequireRestoreConsent=false

REM %NUGET% update packages.config -Verbose -Source %NUGET_SRC% -repositoryPath %NUGET_PKG%

%MSBUILD% %PROJECT% ^
/p:V1BuildToolsPath=%BUILD_TOOLS% ^
/p:NuGetExePath=%NUGET% ^
/p:PackageSources=%NUGET_SRC% ^
/p:RequireRestoreConsent=false ^
/p:Configuration=Release ^
/p:Platform=AnyCPU ^
/p:Major=1 ^
/p:Minor=0 ^
/p:Revision=1 ^
/p:AssemblyInformationalVersion="See https://github.com/versionone/Platform.V1.ServerConnector/wiki" ^
/p:AssemblyCopyright="Copyright 2008-2013, VersionOne, Inc. Licensed under modified BSD." ^
/p:CompanyName="VersionOne, Inc" ^
/p:AssemblyProduct="VersionOne.ServerConnector" ^
/p:AssemblyTitle="VersionOne ServerConnector" ^
/p:AssemblyDescription="VersionOne ServerConnector Release Build" ^
/p:SignAssembly=%SIGN_ASSEMBLY% ^
/p:AssemblyOriginatorKeyFile=%SIGNING_KEY%

ENDLOCAL