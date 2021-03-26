@ECHO OFF
REM Copy actual highlighted test outputs from obj\highlighted to TestInputs and overwrite existing files.
XCOPY obj\highlighted\*.highlighted.html .\ /S /Y /F
