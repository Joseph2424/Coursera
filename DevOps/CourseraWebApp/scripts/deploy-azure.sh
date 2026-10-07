#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_FILE="$SCRIPT_DIR/../CourseraWebApp.csproj"
RESOURCE_GROUP="${1:-CourseraGroup}"
APP_NAME="${2:-CourseraWebApp}"
LOCATION="${3:-westus3}"
PLAN_NAME="${4:-CourseraPlan}"
SKU="${5:-F1}"

for dependency in az dotnet zip; do
    if ! command -v "$dependency" >/dev/null 2>&1; then
        printf 'Required command not found: %s\n' "$dependency" >&2
        exit 1
    fi
done

if [[ ! -f "$PROJECT_FILE" ]]; then
    printf 'Project file not found: %s\n' "$PROJECT_FILE" >&2
    exit 1
fi

if ! az account show --output none >/dev/null 2>&1; then
    printf 'Sign in to Azure first by running: az login\n' >&2
    exit 1
fi

if [[ "$(az group exists --name "$RESOURCE_GROUP" --output tsv)" != "true" ]]; then
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none
fi

if ! az appservice plan show --resource-group "$RESOURCE_GROUP" --name "$PLAN_NAME" --output none >/dev/null 2>&1; then
    az appservice plan create \
        --resource-group "$RESOURCE_GROUP" \
        --name "$PLAN_NAME" \
        --location "$LOCATION" \
        --sku "$SKU" \
        --output none
fi

PLAN_IS_LINUX="$(az appservice plan show --resource-group "$RESOURCE_GROUP" --name "$PLAN_NAME" --query reserved --output tsv)"
if [[ "$PLAN_IS_LINUX" == "true" ]]; then
    printf 'App Service plan %s is Linux-based. This script requires a Windows plan for IIS static hosting.\n' "$PLAN_NAME" >&2
    exit 1
fi

if ! az webapp show --resource-group "$RESOURCE_GROUP" --name "$APP_NAME" --output none >/dev/null 2>&1; then
    az webapp create \
        --resource-group "$RESOURCE_GROUP" \
        --plan "$PLAN_NAME" \
        --name "$APP_NAME" \
        --output none
fi

APP_KIND="$(az webapp show --resource-group "$RESOURCE_GROUP" --name "$APP_NAME" --query kind --output tsv)"
if [[ ",$APP_KIND," == *,linux,* ]]; then
    printf 'App Service %s is Linux-hosted. This script requires a Windows App Service for IIS static hosting.\n' "$APP_NAME" >&2
    exit 1
fi

TEMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TEMP_DIR"' EXIT
PUBLISH_DIR="$TEMP_DIR/publish"
PACKAGE="$TEMP_DIR/site.zip"

dotnet publish "$PROJECT_FILE" --configuration Release --output "$PUBLISH_DIR"

if [[ ! -d "$PUBLISH_DIR/wwwroot" ]]; then
    printf 'Publish output does not contain wwwroot: %s\n' "$PUBLISH_DIR" >&2
    exit 1
fi

(
    cd "$PUBLISH_DIR/wwwroot"
    zip -qr "$PACKAGE" .
)

az webapp deploy \
    --resource-group "$RESOURCE_GROUP" \
    --name "$APP_NAME" \
    --src-path "$PACKAGE" \
    --type zip

printf 'Deployed to https://%s.azurewebsites.net\n' "$APP_NAME"