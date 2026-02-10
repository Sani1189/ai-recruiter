# Root directory (assumes script is run from project root)
$root = (Get-Location).Path

# Function to create a new tab command string
function New-TabCommand {
    param(
        [string]$Directory,
        [string]$Title,
        [string]$TabColor,
        [string]$Command
    )
    return "new-tab -d `"$Directory`" --title `"$Title`" --tabColor `"$TabColor`" powershell -NoExit -Command `"$Command`""
}

# Build the WT command as a single string
$wtCommand = (New-TabCommand -Directory "$root\LLMApi" -Title "LLM API" -TabColor "#8A2BE2" -Command "./start.ps1") + " ; " +
             (New-TabCommand -Directory "$root\CSharpSolutions" -Title "C# Web API" -TabColor "#1E90FF" -Command "dotnet run --project Recruiter/WebApi") + " ; " +
             (New-TabCommand -Directory "$root\Frontend" -Title "Frontend" -TabColor "#2ECC71" -Command "bun run dev")

# Open tabs in the current Windows Terminal window (window 0)
Start-Process wt.exe -ArgumentList "-w 0", $wtCommand