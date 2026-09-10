#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
configuration="${1:-Release}"

dotnet test "$script_dir/TiffToPdf.Tests/TiffToPdf.Tests.csproj" \
    --configuration "$configuration"