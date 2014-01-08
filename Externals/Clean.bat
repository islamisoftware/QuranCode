-:START

del /F /S /Q *.tmp
del /F /S /Q *.bak
del /F /S /Q *.ini
del /F /S /Q *.user
del /F /S /Q *.vshost.exe
del /F /S /Q *.vshost.exe.manifest
::del /F /S /Q /AH *.suo

rd /S /Q Evaluator\obj
rd /S /Q MP3Player\obj
rd /S /Q ObjectListView\obj
rd /S /Q Touch\obj
rd /S /Q Version\obj
rd /S /Q WAVMaker\obj

rd /S /Q Evaluator\bin\Debug
rd /S /Q MP3Player\bin\Debug
rd /S /Q ObjectListView\bin\Debug
rd /S /Q Touch\bin\Debug
rd /S /Q Version\bin\Debug
rd /S /Q WAVMaker\bin\Debug

:END
