# mvc-akira-xmpl-0-0-4 — Lemari Akira (CQRS + JWT)

Aplikasi web ASP.NET Core 10 dengan pola **CQRS**, **JWT Bearer**, entitas
delapan meja, isolasi data per toko, dan audit trail (log). Frontend berupa
SPA statis (vanilla JS + Alpine + Axios + daisyUI).

## Struktur solusi

```
mvc-akira-xmpl-0-0-4/
├── src/
│   ├── MvcAkira.Shared/    # Entitas, DbContext, BacaService, TulisService,
│   │                       # OtoritasService, LogService, Seeder, JwtConfig
│   ├── MvcAkira.Auth/      # 5001 — login/register/logout + JWT
│   ├── MvcAkira.Read/      # 5002 — query: list/detail/trash/dashboard
│   ├── MvcAkira.Write/     # 5003 — command: CRUD + trash + log
│   └── MvcAkira.Frontend/  # 5004 — SPA
├── tests/MvcAkira.Tests/   # xUnit (validator, otoritas, baca, dashboard)
├── data/akira-0-0-4.db     # database SQLite tunggal
├── docs/plan/              # blueprint trilogi (0-0-1 .. 0-0-3)
├── pending/CATATAN-PROGRES.md
├── run.sh                  # jalankan semua service (normal)
├── run-watch.sh            # jalankan semua service (dotnet watch)
└── stop-watch.sh           # matikan semua service
```

## Menjalankan

```bash
./run.sh          # semua service di background, cek /health
# atau dev dengan reload:
./run-watch.sh
# hentikan:
./stop-watch.sh
```

Setelah jalan, buka `http://localhost:5004`. Dokumentasi API (Scalar) tersedia
di `http://localhost:5001/scalar/v1` (Auth), `:5002`, `:5003`.

## Akun seeder

| Email | Sandi | Peran |
|---|---|---|
| kobo.kanaeru@developer.com | kobopawanghujan | superuser (developer) |
| kasir.pepper.lunch.mog@gmail.com | 12345678 | kasir PLMOG |

## Konfigurasi JWT

- Key default di `appsettings.json` tiap service (JWT:Key).
- Produksi: set env `AKIRA_JWT_KEY` (menimpa key dari file konfigurasi).

## Test

```bash
dotnet test tests/MvcAkira.Tests
```

## Endpoint ringkas

Auth (`:5001`)
- `POST /api/auth/login` / `register` / `logout`

Read (`:5002`, Authorization: Bearer)
- `GET /api/dashboard`
- `GET /api/meja_toko` (+ `?code=` detail, `/trash`)
- `GET /api/meja_jabatan` (+ detail)
- `GET /api/meja_target`
- `GET /api/meja_pengguna`
- `GET /api/meja_biodata`
- `GET /api/meja_hakakses`
- `GET /api/meja_keuangan`
- `GET /api/meja_log`
- List *mendukung* `?page=&limit=&search=&sort=&dir=`
  (limit hanya 5/25/50/75/100, selain itu 400)

Write (`:5003`, `:Bearer`)
- `POST/PUT /api/meja_toko` — create/update
- `POST /api/meja_toko/soft-delete|restore`; `DELETE /api/meja_toko/permanent`
- `POST /api/meja_jabatan` (+ soft-delete/restore/permanent)
- `POST /api/meja_target` (+ soft-delete/restore/permanent)
- `POST /api/meja_pengguna/nonaktif` ; `POST /api/meja_pengguna/reset-password`
- `POST /api/meja_biodata` (+ soft-delete/restore/permanent)
- `POST /api/meja_hakakses` (upsert) ; `POST /api/meja_hakakses/soft-delete`
- `POST/PUT /api/meja_keuangan` (+ soft-delete/restore/permanent)

Catatan:
- Teleport soft-delete (sampah) menerima `code` sebagai **JSON string**
  (`-d "\"KODE\""`), bukan object `{"code": ...}`.
- Semua aksi tulis & login dicatat ke `meja_log`.