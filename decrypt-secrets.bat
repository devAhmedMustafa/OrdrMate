@echo off
setlocal enabledelayedexpansion

echo Decrypting appsettings.Development.json...
gpg --output OrdrMate\appsettings.Development.json --decrypt secure\appsettings.Development.json.gpg

if %ERRORLEVEL% NEQ 0 (
  echo ❌ ERROR decrypting appsettings.Development.json
  pause
  exit /b
)

echo.
echo Decrypting keys...

if not exist OrdrMate\Keys mkdir OrdrMate\Keys

for %%f in (secure\*.gpg) do (
  set "filename=%%~nxf"

  :: Skip appsettings.Development.json.gpg
  if /I not "!filename!"=="appsettings.Development.json.gpg" (
    set "nameOnly=%%~nf"
    echo Decrypting %%f to OrdrMate\Keys\!nameOnly!...
    gpg --output OrdrMate\Keys\!nameOnly! --decrypt "%%f"

    if !ERRORLEVEL! NEQ 0 (
      echo ❌ ERROR decrypting %%f
      pause
      exit /b
    )
  )
)

echo.
echo ✅ All decryption done successfully.
pause > nul
