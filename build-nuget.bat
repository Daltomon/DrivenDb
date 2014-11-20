@echo off

SET PATH=.\3rd\NuGet
SET EnableNuGetPackageRestore=true

NuGet Update -self
NuGet Pack .\3rd\NuGet\DrivenDb.nuspec -OutputDirectory .\Release

ECHO.
ECHO.

SET /p PUBLISH="Publish to NuGet (y/n): " %=%

ECHO.
ECHO.

IF "%PUBLISH%" == "y" (
	ECHO Publishing to Nuget...
<<<<<<< HEAD
	NuGet Push .\release\DrivenDb.2.0.nupkg
=======
	NuGet Push .\release\DrivenDb.1.17.nupkg
>>>>>>> origin/master
	ECHO Publish complete.
) ELSE (
	ECHO Publish cancelled.
)

ECHO.
ECHO.

PAUSE