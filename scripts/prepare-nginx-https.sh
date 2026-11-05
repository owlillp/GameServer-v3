#!/usr/bin/env bash
set -euo pipefail

# Генерирует локальную HTTPS-пару для nginx (localhost.pem + localhost.key).
# Файлы лежат вне Git: nginx/certificates/ в .gitignore.
# Использование: ./scripts/prepare-nginx-https.sh

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERTIFICATE_DIR="${1:-$SCRIPT_DIR/../nginx/certificates}"
CERTIFICATE_PATH="$CERTIFICATE_DIR/localhost.pem"
PRIVATE_KEY_PATH="$CERTIFICATE_DIR/localhost.key"

mkdir -p "$CERTIFICATE_DIR"

certificate_exists=false
private_key_exists=false

[ -f "$CERTIFICATE_PATH" ] && certificate_exists=true
[ -f "$PRIVATE_KEY_PATH" ] && private_key_exists=true

if [ "$certificate_exists" != "$private_key_exists" ]; then
    echo "Only one of the expected certificate/key files exists." >&2
    exit 1
fi

if [ "$certificate_exists" = false ]; then
    # dotnet dev-certs создаёт localhost-сертификат, доверяет ему в системном
    # хранилище и экспортирует PEM-пару для nginx.
    if ! dotnet dev-certs https --trust --export-path "$CERTIFICATE_PATH" --format PEM --no-password; then
        echo "dotnet dev-certs --trust недоступен (Linux?). Экспортируем без доверия —" >&2
        echo "браузеру сертификат нужно будет доверить вручную." >&2
        dotnet dev-certs https --export-path "$CERTIFICATE_PATH" --format PEM --no-password
    fi
fi

if [ ! -f "$CERTIFICATE_PATH" ] || [ ! -f "$PRIVATE_KEY_PATH" ]; then
    echo "Failed to prepare the certificate and private key for local nginx." >&2
    exit 1
fi

echo "HTTPS certificate prepared: $CERTIFICATE_PATH"
echo "Next: docker compose up -d --build"
