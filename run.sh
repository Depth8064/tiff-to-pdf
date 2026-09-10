#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
configuration="${1:-Release}"

"$script_dir/build.sh" "$configuration"

exe="$script_dir/bin/$configuration/net10.0-windows/win-x64/publish/TiffToPdf.exe"
"$exe"