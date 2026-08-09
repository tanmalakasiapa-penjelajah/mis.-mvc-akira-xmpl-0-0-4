#!/usr/bin/env bash
#
# Jalankan seluruh layanan MvcAkira.Xmpl (Auth/Read/Write/Frontend) sekaligus
# dalam mode normal (tanpa dotnet watch) di background, log ke logs/*.log.
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

# Pastikan tidak ada layanan yang masih berjalan.
"$ROOT/stop-watch.sh" || true

for svc in Auth Read Write Frontend; do
  port="${PORTS[$svc]}"
  echo "Memulai MvcAkira.$svc di :$port  ->  logs/$svc.log"
  setsid nohup dotnet run --project "$ROOT/src/MvcAkira.$svc" --urls "http://localhost:$port" \
      > "$ROOT/logs/$svc.log" 2>&1 < /dev/null &
done

echo
echo "Menunggu layanan siap..."
for svc in Auth Read Write Frontend; do
  port="${PORTS[$svc]}"
  for i in $(seq 1 40); do
    if curl -sf -o /dev/null "http://localhost:$port/health"; then
      echo "  * $svc  OK  http://localhost:$port"
      break
    fi
    sleep 1
  done
done
echo
echo "Frontend: http://localhost:5004"