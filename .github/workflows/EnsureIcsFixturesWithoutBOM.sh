#!/usr/bin/env bash
# Ensures that no *.ics fixture file in the test project was stored with a byte order mark (BOM).
# Usage: EnsureIcsFixturesWithoutBOM.sh [path-to-scan]
# Defaults to scanning ./Ical.Net.Tests if no path is provided.

set -euo pipefail

scan_path="${1:-Ical.Net.Tests}"

if [ ! -d "$scan_path" ]; then
  echo "::error::Path '$scan_path' does not exist"
  exit 1
fi

found_bom=0
while IFS= read -r -d '' file; do
  # Read first 4 bytes as hex to detect any known BOM signature
  bytes=$(head -c 4 "$file" | od -An -tx1 | tr -d ' \n')

  bom_name=""
  case "$bytes" in
	0000feff*)    bom_name="UTF-32 (BE)" ;;
	fffe0000*)    bom_name="UTF-32 (LE)" ;;
	efbbbf*)      bom_name="UTF-8" ;;
	feff*)        bom_name="UTF-16 (BE)" ;;
	fffe*)        bom_name="UTF-16 (LE)" ;;
  esac

  if [ -n "$bom_name" ]; then
	echo "::error file=$file::File contains a $bom_name BOM"
	found_bom=1
  fi
done < <(find "$scan_path" -type f -name '*.ics' -print0)

if [ "$found_bom" -eq 1 ]; then
  echo "One or more .ics files contain a byte order mark (BOM). Please remove it."
  exit 1
fi

echo "No BOM found in .ics files under '$scan_path'."
