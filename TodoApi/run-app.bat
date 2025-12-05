@echo off
echo ========================================
echo   Starting Todo App - Full Stack
echo ========================================
echo.
echo Starting .NET API Server...
start "API Server" cmd /k "cd /d %~dp0 && dotnet run"
timeout /t 5 /nobreak > nul
echo.
echo Starting React Client...
start "React Client" cmd /k "cd /d %~dp0ToDoListReact && npm start"
echo.
echo ========================================
echo   Both servers are starting!
echo   API: http://localhost:5140
echo   React: http://localhost:3000
echo ========================================
echo.
echo Press any key to close this window...
pause > nul
