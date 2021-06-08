@ECHO OFF
SET mypath=%~dp0
REM Copy actual transformed test outputs from obj\transformed to TestInputs and overwrite existing files.
XCOPY %mypath:~0,-1%\obj\transformed\*.t.cs %mypath:~0,-1%\TestInputs\ /S /Y /F
