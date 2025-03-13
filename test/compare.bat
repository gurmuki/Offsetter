@echo off

REM Fucking Microsoft and their absolutely shitty batch file language!!!!
REM Creating this file took WAY TOO LONG because of all of the idiosyncrases.

set basedir=C:\_sandbox\GLimC#\test\baseline
set resultsdir=C:\_sandbox\GLimC#\test\results

set basefile=%basedir%\%1.log
set resultfile=%resultsdir%\%1.log

if not exist %resultfile% ( exit /b )

"C:\Program Files\WinMerge\WinMergeU.exe" %basefile% %resultfile%

set /p "update=Update baseline? [y/n] "
if %update% NEQ y ( exit /b )

xcopy %resultfile% %basedir% /Y
del %resultsdir%\%1.diff
