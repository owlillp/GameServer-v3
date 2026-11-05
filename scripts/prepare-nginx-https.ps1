param(
    [string]$CertificateDirectory = (Join-Path $PSScriptRoot "..\nginx\certificates")
)

$ErrorActionPreference = "Stop"

# nginx reads the certificate and private key from a directory outside Git.
# These files are for local development only and must never be committed.
$certificatePath = Join-Path $CertificateDirectory "localhost.pem"
$privateKeyPath = Join-Path $CertificateDirectory "localhost.key"

New-Item -ItemType Directory -Path $CertificateDirectory -Force | Out-Null

$certificateExists = Test-Path -LiteralPath $certificatePath
$privateKeyExists = Test-Path -LiteralPath $privateKeyPath

if ($certificateExists -ne $privateKeyExists) {
    throw "Only one of the expected certificate/key files exists."
}

if (-not $certificateExists) {
    # dotnet dev-certs creates a localhost certificate, trusts it in the current
    # user store, and exports the localhost.pem + localhost.key pair for nginx.
    dotnet dev-certs https --trust --export-path $certificatePath --format PEM --no-password

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet dev-certs failed to export the local HTTPS certificate."
    }
}

if (-not (Test-Path -LiteralPath $certificatePath) -or
    -not (Test-Path -LiteralPath $privateKeyPath)) {
    throw "Failed to prepare the certificate and private key for local nginx."
}

Write-Host "HTTPS certificate prepared: $certificatePath"
Write-Host "Next: docker compose up -d --build"
