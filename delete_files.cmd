@echo OFF
REM echo DATFILES_DIR		  %DATFILES_DIR%
REM echo INSTALL_DIR		  %INSTALL_DIR%

del /F /Q %INSTALL_DIR%\guardian\config\*.*
del /F /Q %INSTALL_DIR%\datfiles.zip

del /F /Q %DATFILES_DIR%\*.rsk
del /F /Q %DATFILES_DIR%\ttlicense.lmt

