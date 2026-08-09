#!/usr/bin/env bash
#
# Hentikan semua proses MvcAkira.* (dotnet run / dotnet watch / binary app).
#
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "Menghentikan layanan MvcAkira..."

# 1) dotnet watch run ("MvcAkira.Auth" di baris perintah)
pkill -f "MvcAkira\.(Auth|Read|Write|Frontend)" 2>/dev/null || true

# 2) dotnet run --project (parent)
for svc in Auth Read Write Frontend; do
  pkill -f "$ROOT/src/MvcAkira.$svc" 2>/dev/null || true
done

# 3) Sisa binary app yang mungkin yatim
pkill -f "bin/Debug/net10\.0/MvcAkira\." 2>/dev/null || true

sleep 2

echo "Layanan tersisa:"
pgrep -af "MvcAkira\.(Auth|Read|Write|Frontend)" 2>/dev/null | grep -v "grep" || echo "  (tidak ada)"