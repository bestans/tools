@echo off  
:set CurrentDir=%~dp0

set protoExe=%1
set SRC_DIR=%2
set DST_DIR=%3

echo %protoExe%
echo %SRC_DIR%
echo %DST_DIR%

%protoExe% -I=%SRC_DIR% --python_out=%DST_DIR% %SRC_DIR%/all_config.proto
%protoExe% -I=%SRC_DIR% --csharp_out=%DST_DIR% %SRC_DIR%/all_config.proto