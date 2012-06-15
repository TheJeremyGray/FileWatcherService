@echo off

SET PROG=""
SET FIRSTPART=%WINDIR%"\Microsoft.NET\Framework\v"
SET SECONDPART="\InstallUtil.exe"
SET DOTNETVER=2.0.50727
  IF EXIST %FIRSTPART%%DOTNETVER%%SECONDPART% GOTO install
SET DOTNETVER=1.1.4322
  IF EXIST %FIRSTPART%%DOTNETVER%%SECONDPART% GOTO install
SET DOTNETVER=1.0.3705
  IF EXIST %FIRSTPART%%DOTNETVER%%SECONDPART% GOTO install
GOTO fail
:install
  ECHO Found .NET Framework version %DOTNETVER%
  ECHO Uninstalling service %PROG%
  %FIRSTPART%%DOTNETVER%%SECONDPART% /U %PROG%
  GOTO end
:fail
  echo FAILURE -- Could not find .NET Framework install
:param_error
  echo USAGE: installNETservie.bat [install type (I or U)] [application (.exe)]
:end
  ECHO DONE!!!
  Pause