#!/usr/bin/env bash
#
# Jalankan seluruh layanan MvcAkira.Xmpl dengan `dotnet watch` di background
# (auto-reload saat file berubah), log ke logs/*.log.
#
# Catatan: watch memakai port dari launchSettings.json masing-masing project.
#
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
mkdir -p "$ROOT/logs"

declare -A PORTS=(
  [Auth]=5001
  [Read]=5002
  [Write]=5003
  [Frontend]=5004
)

"$ROOT/stop-watch.sh" || true

for svc in Auth Read Write Frontend; do
  port="${PORTS[$svc]}"
  echo "Memulai dotnet watch MvcAkira.$svc di :$port  ->  logs/watch-$svc.log"
  setsid nohup dotnet watch run --project "$ROOT/src/MvcAkira.$svc" \
      --urls "http://localhost:$port" \
      > "$ROOT/logs/watch-$svc.log" 2>&1 < /dev/null &
done

echo
echo "Menunggu layanan siap (dotnet watch first build)..."
for svc in Auth Read Write Frontend; do
  port="${PORTS[$svc]}"
  for i in $(seq 1 60); do
    if curl -sf -o /dev/null "http://localhost:$port/health"; then
      echo "  * $svc  OK  http://localhost:$port"
      break
    fi
    sleep 1
  done
done
echo
echo "Frontend: http://localhost:5004"