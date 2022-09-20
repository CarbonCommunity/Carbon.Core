@echo off

::  ______ _______ ______ ______ _______ _______ 
:: |      |   _   |   __ \   __ \       |    |  |
:: |   ---|       |      <   __ <   -   |       |
:: |______|___|___|___|__|______/_______|__|____|
::         github.com/Carbon-Modding/Carbon.Core

set BASE=%~dp0

pushd %BASE%..\..\..
set ROOT=%CD%
popd

rem Download rust binary libs
%ROOT%\Tools\DepotDownloader\DepotDownloader\bin\Release\net6.0\DepotDownloader.exe ^
	-app 258550 -branch public -depot 258551 -filelist ^
	%ROOT%\Tools\Helpers\258550_258551_refs.txt -dir %ROOT%\Rust

rem Show me all you've got baby
%ROOT%\Tools\NStrip\NStrip\bin\Release\net452\NStrip.exe ^
	-p -cg --keep-resources -n --unity-non-serialized ^
	%ROOT%\Rust\RustDedicated_Data\Managed\Assembly-CSharp.dll ^
	%ROOT%\Rust\RustDedicated_Data\Managed\Assembly-CSharp.dll