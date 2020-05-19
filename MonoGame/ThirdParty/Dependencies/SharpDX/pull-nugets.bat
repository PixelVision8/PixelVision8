@echo off
setlocal

FOR /F %%i IN (packages.txt) DO (
    call nuget install %%i -Version %1 
    call cp %%i.%1\lib\netstandard1.1\* .\
)

endlocal
