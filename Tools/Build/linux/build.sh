#!/usr/bin/env bash

###
### Copyright (c) 2022 Carbon Community 
### All rights reserved
###
set -e

echo "** Get the base path of the script"
BASE="$(cd -- "$(dirname "$0")" >/dev/null 2>&1; pwd -P)"
ROOT="$(realpath "${BASE}/../../../")"

TARGET=${1:-Debug}
echo "** Set the build target config to ${TARGET}"

echo "** Cleanup the release folder"
rm -rf "${ROOT}/Release/.tmp/${TARGET}" "${ROOT}/Release/Carbon.${TARGET}.tar.gz" || exit 0

echo "** Build the solution"
dotnet restore "${ROOT}/Carbon.Core" -v:m --nologo
dotnet   clean "${ROOT}/Carbon.Core" -v:m --configuration ${TARGET} --nologo
dotnet   build "${ROOT}/Carbon.Core" -v:m --configuration ${TARGET} --no-restore --no-incremental

if [[ "${TARGET}" == *"Unix"* ]]; then
	echo "** Copy doorstop helper files (unix)"
	cp "${ROOT}/Tools/Helpers/environment.sh" "${ROOT}/Release/.tmp/${TARGET}/carbon/tools"
	cp "${ROOT}/Tools/Helpers/publicizer.sh" "${ROOT}/Release/.tmp/${TARGET}/carbon/tools"
	cp "${ROOT}/Tools/UnityDoorstop/linux/x64/libdoorstop.so" "${ROOT}/Release/.tmp/${TARGET}/libdoorstop.so"
else
	echo "** Copy doorstop helper files (windows)"
	cp "${ROOT}/Tools/Helpers/doorstop_config.ini" "${ROOT}/Release/.tmp/${TARGET}"
	cp "${ROOT}/Tools/UnityDoorstop/windows/x64/doorstop.dll" "${ROOT}/Release/.tmp/${TARGET}/winhttp.dll"
fi

echo "** Create the standalone files" 
cp "${ROOT}/Release/.tmp/${TARGET}/HarmonyMods/Carbon.Stub.dll"        "${ROOT}/Release"

cp "${ROOT}/Release/.tmp/${TARGET}/carbon/managed/Carbon.dll"          "${ROOT}/Release"
cp "${ROOT}/Release/.tmp/${TARGET}/carbon/managed/Carbon.Doorstop.dll" "${ROOT}/Release"
cp "${ROOT}/Release/.tmp/${TARGET}/carbon/managed/Carbon.Loader.dll"   "${ROOT}/Release"

cp "${ROOT}/Release/.tmp/${TARGET}/carbon/managed/hooks/Carbon.Hooks.Base.dll"     "${ROOT}/Release"
cp "${ROOT}/Release/.tmp/${TARGET}/carbon/managed/hooks/Carbon.Hooks.Extended.dll" "${ROOT}/Release"


echo "** Create the compressed archive"
tar -zcvf "${ROOT}/Release/Carbon.${TARGET}.tar.gz" -C "${ROOT}/Release/.tmp/${TARGET}" $(ls -A ${ROOT}/Release/.tmp/${TARGET})