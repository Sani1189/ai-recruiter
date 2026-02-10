@echo off
REM Batch file to activate virtual environment and start Azure Functions
REM Usage: start.bat

echo Checking Python version...
python --version

echo Activating virtual environment...

REM Check if virtual environment exists and verify Python version
if not exist ".venv\Scripts\activate.bat" (
    echo Virtual environment not found. Creating with Python 3.10...
    python -m venv .venv
    echo Installing dependencies...
    call .venv\Scripts\activate.bat
    python -m pip install --upgrade pip
    python -m pip install -r requirements.txt
) else (
    REM Verify venv Python version matches .python-version
    for /f "tokens=*" %%i in ('.venv\Scripts\python.exe --version') do set VENV_VERSION=%%i
    echo Virtual environment Python: %VENV_VERSION%
    call .venv\Scripts\activate.bat
)

echo Starting Azure Functions...
REM Set Python path to use venv
set PYTHONPATH=.venv\Lib\site-packages;%PYTHONPATH%
func start --python

