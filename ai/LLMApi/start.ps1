# PowerShell script to activate virtual environment and start Azure Functions
# Usage: .\start.ps1

Write-Host "Checking Python version..." -ForegroundColor Cyan
$pythonVersion = python --version
Write-Host "Using: $pythonVersion" -ForegroundColor Green

Write-Host "Activating virtual environment..." -ForegroundColor Green

# Check if virtual environment exists
if (-not (Test-Path ".venv\Scripts\Activate.ps1")) {
    Write-Host "Virtual environment not found. Creating with Python 3.10..." -ForegroundColor Yellow
    python -m venv .venv
    Write-Host "Installing dependencies..." -ForegroundColor Yellow
    . .venv\Scripts\Activate.ps1
    python -m pip install --upgrade pip
    python -m pip install -r requirements.txt
} else {
    # Verify venv Python version
    $venvVersion = .venv\Scripts\python.exe --version
    Write-Host "Virtual environment Python: $venvVersion" -ForegroundColor Cyan
    . .venv\Scripts\Activate.ps1
}

Write-Host "Starting Azure Functions..." -ForegroundColor Green
func start --python

