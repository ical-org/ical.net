$ErrorActionPreference = "Stop"

function testme() {
    dotnet test .\Ical.Net.CoreUnitTests\
    if ($LASTEXITCODE) {
        throw "Error test"
    }
}

try {
    testme
}
catch {
    Write-Host "Exception: $_"
    exit $LASTEXITCODE
}
