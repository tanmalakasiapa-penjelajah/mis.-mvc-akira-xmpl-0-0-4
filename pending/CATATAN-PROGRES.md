# CATATAN PROGRES — mvc-akira-xmpl-0-0-4

> Dokumen ini berisi status pekerjaan sampai terakhir dikerjakan, hal yang SUDAH
> selesai, yang BELUM/sisa (pending), serta catatan penting yang harus dibaca
> sebelum lanjut besok.
>
> Tanggal catatan: 2026-08-08 (ditutup malam)

---

## 1. Status keseluruhan : ±95% selesai (sisa pembungkus + test otomatis)

Semua fungsionalitas inti selesai & teruji manual via curl :
- Auth / Read / Write / Frontend  : semua endpoint jalan, port 5001-5004
- Isolasi data per toko ("ber-toko") sudah benar (kasir melihat data tokonya sendiri).
- Dashboard sudah terisolasi per toko untuk user biasa.
- Soft-delete ("Sampah") sudah diperbaiki & lengkap di semua tabel (lihat §2 baru).
- Seluruh akun seeder sudah dibekali hak login (login saja) — bisa masuk semua.

### Sisa file/pekerjaan (pending) — prioritas 1 .. 3 :

| No | Item | Status | Folder |
|----|------|--------|--------|
| 1  | Skrip `run.sh`, `run-watch.sh`, `stop-watch.sh` | DIBUAT & TERUJI (jalankan ulang di akhir sesi) | root proyek |
| 2  | Test otomatis `tests/MvcAkira.Tests` | **49 test LOLOS** (validator, otoritas, baca, tulis) | tests |
| 3  | Dokumentasi | **DIBUAT**: `docs/README.md`, `report/`, `bug/`, `walktrough/`, `sketesa-pohon-file.txt` | docs + Dokumen/tempatku-belajar |
| 4  | Hak akses lengkap akun lain | **HANYA login** yang diberikan; fitur belum lengkap → pengguna biasa bisa masuk tapi menu kosong samai di-grant read/create dkk | — |

---

## 2. SUDAH SELESAI & TERUJI (terakhir dikerjakan 2026-08-08 malam)

### 2.1 Soft-delete/Sampah — selesai penuh
- **Bug akar**: frontend mengirim `code` mentah utk `[FromBody] string` → HTTP 400.
  Fix: `JSON.stringify(code)` + header `Content-Type: application/json`
  (di `app.js`: `softDelete`, `restoreTrash`, `permanentTrash`; `config.js`: create jabatan).
- Backend baru di `BacaService` + `ReadController`: endpoint `/api/{tabel}/trash` untuk
  semua tabel (toko, jabatan, target, pengguna, biodata, hak akses, keuangan).
- `TulisService` + `WriteController`: tambah Create/Update yang kurang
  (Jabatan, Target, Pengguna, Biodata) + SoftDelete/Restore/Permanent utk pengguna
  dan hak akses. Guard ANTI_ORPHAN (menolak soft-delete pengguna yg masih punya
  hak aktif); saat soft-delete pengguna otomatis nonaktif=1.
- Frontend: halaman **Sampah** (route `sampah`) dengan tab per tabel,
  tombol Pulihkan & Hapus Selamanya, paginasi. Flag `hapus:false`/`readOnly`
  mengontrol tombol di halaman biasa (meja_log read-only).
- Test TulisService baru: jumlah test sekarang 49, SEMUA lolos. Build semua sukses.

### 2.2 Login akun lain
- Problem: akun selain Kobo `akun tidak aktif` / `belum memiliki hak login`.
- Solusi: upsert `meja_hakakses` target `meja_pengguna` dengan flag login saja.
- Catatan: upsert MENIMPA seluruh baris — saat mengubah satu flag harus sertakan
  semua 5 flag (read/create/update/delete/login); Kobo sempat ke-reset → diperbaiki.

### 2.3 Dokumentasi (malam)
- `~/Dokumen/tempatku-belajar/mvc-akira-xmpl-0-0-4/` :
    - `report/report-2026-08-08.txt`   : ringkasan pekerjaan hari ini
    - `bug/daftar-bug-2026-08-08.txt`  : 5 bug + solusi
    - `walktrough/model.txt`, `view.txt`, `controller.txt`, `manual-service.txt`
    - `sketesa-pohon-file.txt`         : tree + kegunaan tiap file

---

## 3. CATATAN PENTING (bug/perilaku yang perlu diketahui)

- Endpoint soft-delete/restore/permanent menerima `code` sebagai **JSON string**
  (`-d "\"KODE...\""`), bukan objek. Body `[FromBody] string name` (create jabatan)
  juga JSON string ter-quote.
- `UpsertHakaksesAsync` MENIMPA seluruh flag baris; saat mengubah satu flag,
  sertakan kelima flag dari kondisi lama (baca dulu via Read).
- `IsMasterSensitif` (Toko/Jabatan/Target) → boleh diubah hanya superuser.
- `dotnet watch` tidak selalu memuat tipe/route baru → jalankan restart penuh
  (`./stop-watch.sh && ./run-watch.sh`) setelah ada file baru.
- Limit halaman list hanya `5/25/50/75/100`; selain itu HTTP 400.
- Port: Auth 5001, Read 5002, Write 5003, Frontend 5004.
- Akun seeder sandi: Kobo `kobopawanghujan`; lainnya `12345678` (nonaktif msb 0).

---

## 4. YANG BELUM / NEXT STEP (besok)

1. **Hak akses & fitur di UI**: akun biasa sekarang hanya login; beri read/create
   per target secara babak-babi bila mau lengkap ("semua fitur belum lengkap"
   sesuai keinginan user malam ini).
2. Verifikasi UI via browser (zen/firefox) untuk halaman Sampah & modal; malam ini
   hanya curl + syntax check JS yang jalan.
3. (Opsional) filter tanggal di sampah, hapus button pada History, dll.

```bash
# cepat cek semua layanan
./stop-watch.sh && ./run-watch.sh
for p in 5001 5002 5003 5004; do
  echo -n "$p: "; curl -s -o /dev/null -w "%{http_code}\n" localhost:$p/health
done
```

---

## 5. SUSUNAN FOLDER (agar paham letak)
```
mvc-akira-xmpl-0-0-4/
  src/
    MvcAkira.Auth/      (port 5001, JWT login/register/logout)
    MvcAkira.Read/      (port 5002, semua list + trash + dashboard)
    MvcAkira.Write/     (port 5003, CRUD + sampah)
    MvcAkira.Frontend/  (port 5004, SPA)
    MvcAkira.Shared/    (entity, DbContext, BacaService, TulisService, Otoritas, Log)
  tests/MvcAkira.Tests/ (49 test)
  data/akira-0-0-4.db
  docs/plan/ (plan-0-0-1..3 dari folder analisa)
  pending/   (dokumen ini)
  run.sh / run-watch.sh / stop-watch.sh
  report/ bug/ walktrough/ + sketesa-pohon-file.txt (di ~/Dokumen/tempatku-belajar/...)
```