::
:: Copyright (c) 2022 Carbon Community 
:: All rights reserved
::
@echo off
set BASE=%~dp0

echo ** Get the base path of the script
pushd %BASE%..\..\..
set ROOT=%CD%
popd

if "%1" EQU "" (
	set TARGET=Debug
) else (
	set TARGET=%1
)
echo ** Set the build target config to %TARGET%

echo ** Cleanup the release folder
rmdir /s /q "%ROOT%\Release\.tmp\%TARGET%" 2>NUL
del /q "%ROOT%\Release\Carbon.%TARGET%.zip" 2>NUL

echo ** Build the solution
dotnet restore "%ROOT%\Carbon.Core" -v:m --nologo || exit /b
dotnet   clean "%ROOT%\Carbon.Core" -v:m --configuration %TARGET% --nologo || exit /b
dotnet   build "%ROOT%\Carbon.Core" -v:m --configuration %TARGET% --no-restore --no-incremental || exit /b

echo ** Copy operating system specific files
echo "%TARGET%" | findstr /C:"Unix" >NUL && (
	copy /y "%ROOT%\Tools\Helpers\environment.sh"                 "%ROOT%\Release\.tmp\%TARGET%\carbon\tools\"
	copy /y "%ROOT%\Tools\Helpers\publicizer.sh"                  "%ROOT%\Release\.tmp\%TARGET%\carbon\tools\"
	copy /y "%ROOT%\Tools\UnityDoorstop\linux\x64\libdoorstop.so" "%ROOT%\Release\.tmp\%TARGET%"
	(CALL )
) || (
	copy /y "%ROOT%\Tools\Helpers\doorstop_config.ini"            "%ROOT%\Release\.tmp\%TARGET%"
	copy /y "%ROOT%\Tools\UnityDoorstop\windows\x64\doorstop.dll" "%ROOT%\Release\.tmp\%TARGET%\winhttp.dll"
	(CALL )
)

echo ** Create the standalone files
copy /y "%ROOT%\Release\.tmp\%TARGET%\HarmonyMods\Carbon.Stub.dll"        "%ROOT%\Release"

copy /y "%ROOT%\Release\.tmp\%TARGET%\carbon\managed\Carbon.dll"          "%ROOT%\Release"
copy /y "%ROOT%\Release\.tmp\%TARGET%\carbon\managed\Carbon.Doorstop.dll" "%ROOT%\Release"
copy /y "%ROOT%\Release\.tmp\%TARGET%\carbon\managed\Carbon.Loader.dll"   "%ROOT%\Release"

copy /y "%ROOT%\Release\.tmp\%TARGET%\carbon\managed\hooks\Carbon.Hooks.Base.dll"     "%ROOT%\Release"
copy /y "%ROOT%\Release\.tmp\%TARGET%\carbon\managed\hooks\Carbon.Hooks.Extended.dll" "%ROOT%\Release"


echo ** Create the compressed archive
powershell -Command "Compress-Archive -Update -Path '%ROOT%\Release\.tmp\%TARGET%\*' -DestinationPath '%ROOT%\Release\Carbon.%TARGET%.zip'"
