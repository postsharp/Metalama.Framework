@ECHO OFF
REM Copy actual transformed test outputs from obj\transformed to TestInputs and overwrite existing files.
XCOPY obj\transformed\*.transformed.txt .\TestInputs\ /S /Y /F
