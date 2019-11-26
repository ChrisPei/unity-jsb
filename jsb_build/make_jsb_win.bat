@echo off

python jsb_helpers.py version .\quickjs-2019-10-27\VERSION .\jsb\jsb_version.h

mkdir out
pushd out

rd /s /q win_x64
mkdir win_x64
pushd win_x64
REM cmake -G "Visual Studio 15 2017 Win64" ..\..\jsb
cmake -G "Visual Studio 16 2019" -A x64 ..\..\jsb
popd
cmake --build win_x64 --config Release
popd

xcopy /Y .\out\win_x64\Release\jsb.dll ..\Assets\jsb\Plugins\x64\