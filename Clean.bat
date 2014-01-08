-:START

del *.zip
del *.rar
del /F /S /Q *.tmp
del /F /S /Q *.bak
del /F /S /Q *.ini
del /F /S /Q *.user
del /F /S /Q *.vshost.exe
del /F /S /Q *.vshost.exe.manifest
::del /F /S /Q /AH *.suo

rd /S /Q Build\Debug

del /F /S /Q Build\Release\*.pdb
del /F    /Q Build\Release\Translations\*.txt
rd /S /Q Build\Release\Bookmarks
rd /S /Q Build\Release\History
rd /S /Q Build\Release\Statistics
rd /S /Q Build\Release\Drawings
rd /S /Q Build\Release\Research

rd /S /Q Globals\obj
rd /S /Q Utilities\obj
rd /S /Q Model\obj
rd /S /Q DataAccess\obj
rd /S /Q Server\obj
rd /S /Q Client\obj
rd /S /Q QuranCode\obj
rd /S /Q Research\obj
rd /S /Q ScriptRunner\obj
rd /S /Q InitialLetters\obj
rd /S /Q PrimeCalculator\obj

CALL Externals\Clean.bat

:END
