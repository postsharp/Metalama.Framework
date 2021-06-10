@ECHO OFF
REM Copy actual highlighted test outputs from obj\highlighted to TestInputs and overwrite existing files.
XCOPY obj\highlighted\*.t.html .\TestInputs\ /S /Y /F
