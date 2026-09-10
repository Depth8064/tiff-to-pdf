#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
configuration="${1:-Release}"

"$script_dir/test.sh" "$configuration"

dotnet publish "$script_dir/TiffToPdf.csproj" \
    --configuration "$configuration" \
    --runtime win-x64

exe="$script_dir/bin/$configuration/net10.0-windows/win-x64/publish/TiffToPdf.exe"

if [[ ! -f "$exe" ]]; then
    printf 'Publish completed but TiffToPdf.exe was not found.\n' >&2
    exit 1
fi

size_mb="$(du -m "$exe" | cut -f1)"
printf '\nBuilt: %s (%s MB)\n' "$exe" "$size_mb"