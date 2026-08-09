using Microsoft.EntityFrameworkCore;
using MvcAkira.Shared.Contracts;
using MvcAkira.Shared.Data;
using MvcAkira.Shared.Entities;
using MvcAkira.Shared.Enums;
using MvcAkira.Shared.Security;

namespace MvcAkira.Shared.Services;

public static class ListQueryValidator
{
    public static (int Page, int Limit, bool Ok) Normalize(ListQuery q)
    {
        if (q.Limit <= 0 || !ListQuery.AllowedLimits.Contains(q.Limit))
            return (1, 0, false);
        if (q.Page < 1) q.Page = 1;
        return (q.Page, q.Limit, true);
    }
}

/// <summary>Semua query baca (list/search/sort/pagination+ isolasi toko/detail/trash).</summary>
public class BacaService
{
    private readonly AkiraDbContext _db;
    private readonly OtoritasService _otoritas;

    public BacaService(AkiraDbContext db, OtoritasService otoritas)
    {
        _db = db;
        _otoritas = otoritas;
    }

    public async Task<(bool Boleh, string? Pesan)> CekBacaAsync(string targetName, CancellationToken ct)
        => (await _otoritas.BolehBacaAsync(targetName, ct))
            ? (true, null)
            : (false, "tidak memiliki hak baca");

    // ---------- TOKO ----------
    public async Task<PageResult<TokoView>> ListToko(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");

        var baseQuery = _db.MejaToko.AsNoTracking().Where(t => t.TokoSoftdeleted == 0);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.ToLower();
            baseQuery = baseQuery.Where(t =>
                t.TokoName.ToLower().Contains(s) || t.TokoAddress.ToLower().Contains(s)
                || t.TokoEmail.ToLower().Contains(s) || t.TokoPhone.ToLower().Contains(s));
        }
        var total = await baseQuery.CountAsync(ct);
        var ordered = ApplySort_Toko(baseQuery, q.Sort, q.Dir);
        var items = await ordered.Skip((page - 1) * limit).Take(limit).ToListAsync(ct);
        return new PageResult<TokoView>
        {
            List = items.Select(x => x.ToView()).ToList(),
            Total = total, Page = page, Limit = limit,
            TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit),
        };
    }

    public async Task<TokoView?> DetailTokoAsync(string code, CancellationToken ct)
        => (await _db.MejaToko.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokoCode == code && t.TokoSoftdeleted == 0, ct))?.ToView();

    public async Task<PageResult<TokoView>> TrashToko(ListQuery q, CancellationToken ct)
    {
        var (page, limit, _) = ListQueryValidator.Normalize(q);
        var qb = _db.MejaToko.AsNoTracking().Where(t => t.TokoSoftdeleted == 1);
        var total = await qb.CountAsync(ct);
        var items = await qb.Skip((page - 1) * limit).Take(limit).ToListAsync(ct);
        return new PageResult<TokoView>
        { List = items.Select(x => x.ToView()).ToList(), Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<PageResult<JabatanView>> TrashJabatan(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var qb = _db.MejaJabatan.AsNoTracking().Where(j => j.JabatanSoftdeleted == 1);
        var total = await qb.CountAsync(ct);
        var items = await qb.OrderByDescending(x => x.JabatanCreatedat)
            .Skip((page - 1) * limit).Take(limit).ToListAsync(ct);
        return new PageResult<JabatanView>
        { List = items.Select(x => x.ToView()).ToList(), Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<PageResult<TargetView>> TrashTarget(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var qb = _db.MejaTarget.AsNoTracking().Where(t => t.TargetSoftdeleted == 1);
        var total = await qb.CountAsync(ct);
        var items = await qb.OrderByDescending(x => x.TargetCreatedat)
            .Skip((page - 1) * limit).Take(limit).ToListAsync(ct);
        return new PageResult<TargetView>
        { List = items.Select(x => x.ToView()).ToList(), Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<PageResult<PenggunaView>> TrashPengguna(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var items = await _db.MejaPengguna.AsNoTracking()
            .Where(p => p.PenggunaSoftdeleted == 1)
            .OrderByDescending(x => x.PenggunaCreatedat)
            .Skip((page - 1) * limit).Take(limit)
            .Select(x => new PenggunaView(x.PenggunaCode, x.PenggunaEmail, x.PenggunaNonaktif, x.PenggunaCreatedat))
            .ToListAsync(ct);
        var total = await _db.MejaPengguna.CountAsync(p => p.PenggunaSoftdeleted == 1, ct);
        return new PageResult<PenggunaView>
        { List = items, Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<PageResult<BiodataView>> TrashBiodata(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var userToko = await _otoritas.UserTokoCodeAsync(ct);
        var inner = from b in _db.MejaBiodata
                    join p in _db.MejaPengguna on b.PenggunaCode equals p.PenggunaCode
                    join t in _db.MejaToko on b.TokoCode equals t.TokoCode
                    join j in _db.MejaJabatan on b.JabatanCode equals j.JabatanCode
                    where b.BiodataSoftdeleted == 1
                    select new { b, p, t, j };
        if (!string.IsNullOrEmpty(userToko))
            inner = inner.Where(x => x.b.TokoCode == userToko);
        var total = await inner.CountAsync(ct);
        var items = await inner.OrderByDescending(x => x.b.BiodataCreatedat)
            .Skip((page - 1) * limit).Take(limit)
            .Select(x => new BiodataView(x.b.BiodataCode, x.b.PenggunaCode, x.p.PenggunaEmail,
                x.b.TokoCode, x.t.TokoName, x.b.JabatanCode, x.j.JabatanName,
                x.b.BiodataFullname, x.b.BiodataBorn, x.b.BiodataAddress, x.b.BiodataPhone,
                x.b.BiodataCreatedat)).ToListAsync(ct);
        return new PageResult<BiodataView>
        { List = items, Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<PageResult<HakaksesView>> TrashHakakses(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var inner = from h in _db.MejaHakakses
                    join p in _db.MejaPengguna on h.PenggunaCode equals p.PenggunaCode
                    join tg in _db.MejaTarget on h.TargetCode equals tg.TargetCode
                    where h.HakaksesSoftdeleted == 1
                    select new { h, p, tg };
        var total = await inner.CountAsync(ct);
        var items = await inner.OrderBy(x => x.p.PenggunaEmail).ThenBy(x => x.tg.TargetName)
            .Skip((page - 1) * limit).Take(limit)
            .Select(x => new HakaksesView(x.h.HakaksesCode, x.h.PenggunaCode, x.p.PenggunaEmail,
                x.h.TargetCode, x.tg.TargetName, x.h.HakaksesRead, x.h.HakaksesCreate,
                x.h.HakaksesUpdate, x.h.HakaksesDelete, x.h.HakaksesLogin)).ToListAsync(ct);
        return new PageResult<HakaksesView>
        { List = items, Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<PageResult<KeuanganView>> TrashKeuangan(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var userToko = await _otoritas.UserTokoCodeAsync(ct);
        var inner = from k in _db.MejaKeuangan
                    join p in _db.MejaPengguna on k.PenggunaCode equals p.PenggunaCode
                    join t in _db.MejaToko on k.TokoCode equals t.TokoCode
                    where k.KeuanganSoftdeleted == 1
                    select new { k, p, t };
        if (!string.IsNullOrEmpty(userToko))
            inner = inner.Where(x => x.k.TokoCode == userToko);
        var total = await inner.CountAsync(ct);
        var items = await inner.OrderByDescending(x => x.k.KeuanganWaktucatat)
            .Skip((page - 1) * limit).Take(limit)
            .Select(x => new KeuanganView(x.k.KeuanganCode, x.k.PenggunaCode, x.p.PenggunaEmail,
                x.k.TokoCode, x.t.TokoName, x.k.KeuanganNominal, x.k.KeuanganJudul,
                x.k.KeuanganDeskripsi, x.k.KeuanganStatus, x.k.KeuanganTempat,
                x.k.KeuanganWaktucatat, x.k.KeuanganCreatedat)).ToListAsync(ct);
        return new PageResult<KeuanganView>
        { List = items, Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    private static IQueryable<MejaToko> ApplySort_Toko(IQueryable<MejaToko> q, string? sort, string dir)
    {
        var desc = dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort switch
        {
            "name" => desc ? q.OrderByDescending(x => x.TokoName) : q.OrderBy(x => x.TokoName),
            "createdat" => desc ? q.OrderByDescending(x => x.TokoCreatedat) : q.OrderBy(x => x.TokoCreatedat),
            _ => q.OrderByDescending(x => x.TokoCreatedat),
        };
    }

    // ---------- JABATAN ----------
    public async Task<PageResult<JabatanView>> ListJabatan(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var qb = _db.MejaJabatan.AsNoTracking().Where(j => j.JabatanSoftdeleted == 0);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.ToLower();
            qb = qb.Where(j => j.JabatanName.ToLower().Contains(s));
        }
        var total = await qb.CountAsync(ct);
        var desc = q.Dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        var ordered = q.Sort switch
        {
            "name" => desc ? qb.OrderByDescending(x => x.JabatanName) : qb.OrderBy(x => x.JabatanName),
            _ => qb.OrderByDescending(x => x.JabatanCreatedat),
        };
        var items = await ordered.Skip((page - 1) * limit).Take(limit).ToListAsync(ct);
        return new PageResult<JabatanView>
        { List = items.Select(x => x.ToView()).ToList(), Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<JabatanView?> DetailJabatanAsync(string code, CancellationToken ct)
        => await _db.MejaJabatan.AsNoTracking()
            .FirstOrDefaultAsync(j => j.JabatanCode == code && j.JabatanSoftdeleted == 0, ct)
            is { } e ? e.ToView() : null;

    // ---------- TARGET ----------
    public async Task<PageResult<TargetView>> ListTarget(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var qb = _db.MejaTarget.AsNoTracking().Where(t => t.TargetSoftdeleted == 0);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.ToLower();
            qb = qb.Where(t => t.TargetName.ToLower().Contains(s) || t.TargetKeterangan.ToLower().Contains(s));
        }
        var total = await qb.CountAsync(ct);
        var desc = q.Dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        var items = await (q.Sort switch
        {
            "name" => desc ? qb.OrderByDescending(x => x.TargetName) : qb.OrderBy(x => x.TargetName),
            "keterangan" => desc ? qb.OrderByDescending(x => x.TargetKeterangan) : qb.OrderBy(x => x.TargetKeterangan),
            _ => qb.OrderByDescending(x => x.TargetCreatedat),
        }).Skip((page - 1) * limit).Take(limit).ToListAsync(ct);
        return new PageResult<TargetView>
        { List = items.Select(x => x.ToView()).ToList(), Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<TargetView?> DetailTargetAsync(string code, CancellationToken ct)
        => await _db.MejaTarget.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TargetCode == code && t.TargetSoftdeleted == 0, ct)
            is { } e ? e.ToView() : null;

    // ---------- PENGGUNA ----------
    public async Task<PageResult<PenggunaView>> ListPengguna(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var qb = _db.MejaPengguna.AsNoTracking().Where(p => p.PenggunaSoftdeleted == 0);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.ToLower();
            qb = qb.Where(p => p.PenggunaEmail.ToLower().Contains(s));
        }
        var total = await qb.CountAsync(ct);
        var desc = q.Dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        var items = await (q.Sort switch
        {
            "email" => desc ? qb.OrderByDescending(x => x.PenggunaEmail) : qb.OrderBy(x => x.PenggunaEmail),
            "status" => desc ? qb.OrderByDescending(x => x.PenggunaNonaktif) : qb.OrderBy(x => x.PenggunaNonaktif),
            "createdat" => desc ? qb.OrderByDescending(x => x.PenggunaCreatedat) : qb.OrderBy(x => x.PenggunaCreatedat),
            _ => qb.OrderByDescending(x => x.PenggunaCreatedat),
        }).Skip((page - 1) * limit).Take(limit).ToListAsync(ct);
        return new PageResult<PenggunaView>
        { List = items.Select(x => x.ToView()).ToList(), Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<PenggunaView?> DetailPenggunaAsync(string code, CancellationToken ct)
        => await _db.MejaPengguna.AsNoTracking()
            .FirstOrDefaultAsync(p => p.PenggunaCode == code && p.PenggunaSoftdeleted == 0, ct)
            is { } e ? e.ToView() : null;

    // ---------- BIODATA (join, isolasi toko) ----------
    public async Task<PageResult<BiodataView>> ListBiodata(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var userToko = await _otoritas.UserTokoCodeAsync(ct);

        var inner = from b in _db.MejaBiodata
                    join p in _db.MejaPengguna on b.PenggunaCode equals p.PenggunaCode
                    join t in _db.MejaToko on b.TokoCode equals t.TokoCode
                    join j in _db.MejaJabatan on b.JabatanCode equals j.JabatanCode
                    where b.BiodataSoftdeleted == 0
                    select new { b, p, t, j };
        if (!string.IsNullOrEmpty(userToko))
            inner = inner.Where(x => x.b.TokoCode == userToko);

        var total = await inner.CountAsync(ct);
        var qf = inner.AsQueryable();
        if (q.Search is { Length: > 0 })
        {
            var s = q.Search.ToLower();
            qf = qf.Where(x => x.p.PenggunaEmail.ToLower().Contains(s)
                || x.t.TokoName.ToLower().Contains(s) || x.j.JabatanName.ToLower().Contains(s)
                || x.b.BiodataFullname.ToLower().Contains(s) || x.b.BiodataPhone.ToLower().Contains(s));
        }

        var desc = q.Dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        var sel = q.Sort switch
        {
            "toko" => desc
                ? qf.OrderByDescending(x => x.t.TokoName)
                : qf.OrderBy(x => x.t.TokoName),
            "nama" => desc
                ? qf.OrderByDescending(x => x.b.BiodataFullname)
                : qf.OrderBy(x => x.b.BiodataFullname),
            "pengguna" => desc
                ? qf.OrderByDescending(x => x.p.PenggunaEmail)
                : qf.OrderBy(x => x.p.PenggunaEmail),
            "jabatan" => desc
                ? qf.OrderByDescending(x => x.j.JabatanName)
                : qf.OrderBy(x => x.j.JabatanName),
            _ => desc
                ? qf.OrderByDescending(x => x.b.BiodataCreatedat)
                : qf.OrderBy(x => x.b.BiodataCreatedat),
        };
        var items = await sel.Skip((page - 1) * limit).Take(limit)
            .Select(x => new BiodataView(x.b.BiodataCode, x.b.PenggunaCode, x.p.PenggunaEmail,
                x.b.TokoCode, x.t.TokoName, x.b.JabatanCode, x.j.JabatanName,
                x.b.BiodataFullname, x.b.BiodataBorn, x.b.BiodataAddress, x.b.BiodataPhone,
                x.b.BiodataCreatedat)).ToListAsync(ct);
        return new PageResult<BiodataView>
        { List = items, Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<BiodataView?> DetailBiodataAsync(string code, CancellationToken ct)
    {
        var userToko = await _otoritas.UserTokoCodeAsync(ct);
        var q = from b in _db.MejaBiodata
                join p in _db.MejaPengguna on b.PenggunaCode equals p.PenggunaCode
                join t in _db.MejaToko on b.TokoCode equals t.TokoCode
                join j in _db.MejaJabatan on b.JabatanCode equals j.JabatanCode
                where b.BiodataCode == code && b.BiodataSoftdeleted == 0
                select new BiodataView(b.BiodataCode, b.PenggunaCode, p.PenggunaEmail,
                    b.TokoCode, t.TokoName, b.JabatanCode, j.JabatanName,
                    b.BiodataFullname, b.BiodataBorn, b.BiodataAddress, b.BiodataPhone,
                    b.BiodataCreatedat);
        var item = await q.FirstOrDefaultAsync(ct);
        if (item is null) return null;
        return !string.IsNullOrEmpty(userToko) && item.TokoCode != userToko ? null : item;
    }

    // ---------- HAK AKSES (join) ----------
    public async Task<PageResult<HakaksesView>> ListHakakses(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var inner = from h in _db.MejaHakakses
                    join p in _db.MejaPengguna on h.PenggunaCode equals p.PenggunaCode
                    join tg in _db.MejaTarget on h.TargetCode equals tg.TargetCode
                    where h.HakaksesSoftdeleted == 0
                    select new { h, p, tg };
        var total = await inner.CountAsync(ct);
        var qf = inner.AsQueryable();
        if (q.Search is { Length: > 0 })
        {
            var s = q.Search.ToLower();
            qf = qf.Where(x => x.p.PenggunaEmail.ToLower().Contains(s) || x.tg.TargetName.ToLower().Contains(s));
        }
        var desc = q.Dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        var qf2 = qf;
        if (q.Sort == "target")
            qf2 = desc ? qf.OrderByDescending(x => x.tg.TargetName) : qf.OrderBy(x => x.tg.TargetName);
        else if (q.Sort == "pengguna")
            qf2 = desc ? qf.OrderByDescending(x => x.p.PenggunaEmail) : qf.OrderBy(x => x.p.PenggunaEmail);
        else
            qf2 = qf.OrderBy(x => x.p.PenggunaEmail).ThenBy(x => x.tg.TargetName);
        var items = await qf2.Skip((page - 1) * limit).Take(limit)
            .Select(x => new HakaksesView(x.h.HakaksesCode, x.h.PenggunaCode, x.p.PenggunaEmail,
                x.h.TargetCode, x.tg.TargetName, x.h.HakaksesRead, x.h.HakaksesCreate,
                x.h.HakaksesUpdate, x.h.HakaksesDelete, x.h.HakaksesLogin)).ToListAsync(ct);
        return new PageResult<HakaksesView>
        { List = items, Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<HakaksesView?> DetailHakaksesAsync(string code, CancellationToken ct)
        => await (from h in _db.MejaHakakses
                  join p in _db.MejaPengguna on h.PenggunaCode equals p.PenggunaCode
                  join tg in _db.MejaTarget on h.TargetCode equals tg.TargetCode
                  where h.HakaksesCode == code && h.HakaksesSoftdeleted == 0
                  select new HakaksesView(h.HakaksesCode, h.PenggunaCode, p.PenggunaEmail,
                      h.TargetCode, tg.TargetName, h.HakaksesRead, h.HakaksesCreate,
                      h.HakaksesUpdate, h.HakaksesDelete, h.HakaksesLogin))
            .FirstOrDefaultAsync(ct);

    // ---------- KEUANGAN (join, isolasi toko) ----------
    public async Task<PageResult<KeuanganView>> ListKeuangan(ListQuery q, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var userToko = await _otoritas.UserTokoCodeAsync(ct);

        // Gunakan anonymous type utk join agar translasi EF valid, lalu proyeksi terakhir.
        var inner = from k in _db.MejaKeuangan
                    join p in _db.MejaPengguna on k.PenggunaCode equals p.PenggunaCode
                    join t in _db.MejaToko on k.TokoCode equals t.TokoCode
                    where k.KeuanganSoftdeleted == 0
                    select new { k, p, t };
        if (!string.IsNullOrEmpty(userToko))
            inner = inner.Where(x => x.k.TokoCode == userToko);

        var total = await inner.CountAsync(ct);

        var qf = inner.AsQueryable();
        if (q.Search is { Length: > 0 })
        {
            var s = q.Search.ToLower();
            qf = qf.Where(x => x.k.KeuanganJudul.ToLower().Contains(s)
                || x.t.TokoName.ToLower().Contains(s) || x.p.PenggunaEmail.ToLower().Contains(s)
                || x.k.KeuanganStatus.ToLower().Contains(s) || x.k.KeuanganTempat.ToLower().Contains(s));
        }

        var desc = q.Dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        var ordered = q.Sort switch
        {
            "judul" => desc ? qf.OrderByDescending(x => x.k.KeuanganJudul) : qf.OrderBy(x => x.k.KeuanganJudul),
            "nominal" => desc ? qf.OrderByDescending(x => x.k.KeuanganNominal) : qf.OrderBy(x => x.k.KeuanganNominal),
            "status" => desc ? qf.OrderByDescending(x => x.k.KeuanganStatus) : qf.OrderBy(x => x.k.KeuanganStatus),
            "tempat" => desc ? qf.OrderByDescending(x => x.k.KeuanganTempat) : qf.OrderBy(x => x.k.KeuanganTempat),
            "toko" => desc ? qf.OrderByDescending(x => x.t.TokoName) : qf.OrderBy(x => x.t.TokoName),
            "pengguna" => desc ? qf.OrderByDescending(x => x.p.PenggunaEmail) : qf.OrderBy(x => x.p.PenggunaEmail),
            _ => desc ? qf.OrderByDescending(x => x.k.KeuanganWaktucatat) : qf.OrderBy(x => x.k.KeuanganWaktucatat),
        };
        var items = await ordered.Skip((page - 1) * limit).Take(limit)
            .Select(x => new KeuanganView(x.k.KeuanganCode, x.k.PenggunaCode, x.p.PenggunaEmail,
                x.k.TokoCode, x.t.TokoName, x.k.KeuanganNominal, x.k.KeuanganJudul,
                x.k.KeuanganDeskripsi, x.k.KeuanganStatus, x.k.KeuanganTempat,
                x.k.KeuanganWaktucatat, x.k.KeuanganCreatedat)).ToListAsync(ct);

        return new PageResult<KeuanganView>
        { List = items, Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<KeuanganView?> DetailKeuanganAsync(string code, CancellationToken ct)
    {
        var userToko = await _otoritas.UserTokoCodeAsync(ct);
        var item = await (from k in _db.MejaKeuangan
                          join p in _db.MejaPengguna on k.PenggunaCode equals p.PenggunaCode
                          join t in _db.MejaToko on k.TokoCode equals t.TokoCode
                          where k.KeuanganCode == code && k.KeuanganSoftdeleted == 0
                          select new KeuanganView(k.KeuanganCode, k.PenggunaCode, p.PenggunaEmail,
                              k.TokoCode, t.TokoName, k.KeuanganNominal, k.KeuanganJudul,
                              k.KeuanganDeskripsi, k.KeuanganStatus, k.KeuanganTempat,
                              k.KeuanganWaktucatat, k.KeuanganCreatedat)).FirstOrDefaultAsync(ct);
        if (item is null) return null;
        return !string.IsNullOrEmpty(userToko) && item.TokoCode != userToko ? null : item;
    }

    // ---------- LOG (left join pelaku) ----------
    public async Task<PageResult<LogView>> ListLog(ListQuery q, string? dari, string? sampai, CancellationToken ct)
    {
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        if (!ok) throw new ApiException(400, "DATA_TIDAK_LENGKAP", "limit tidak valid");
        var qb = from l in _db.MejaLog
                 join p in _db.MejaPengguna on l.LogPelaku equals p.PenggunaCode into ps
                 from p in ps.DefaultIfEmpty()
                 where l.LogSoftdeleted == 0
                 select new { l, p };
        if (q.Search is { Length: > 0 })
        {
            var s = q.Search.ToLower();
            qb = qb.Where(x => x.p != null && x.p.PenggunaEmail.ToLower().Contains(s)
                || x.l.LogMencatat.ToLower().Contains(s) || x.l.LogTarget.ToLower().Contains(s));
        }
        if (!string.IsNullOrEmpty(dari)) qb = qb.Where(x => String.Compare(x.l.LogCreatedat, dari + " 00:00:00") >= 0);
        if (!string.IsNullOrEmpty(sampai)) qb = qb.Where(x => String.Compare(x.l.LogCreatedat, sampai + " 23:59:59") <= 0);

        var total = await qb.CountAsync(ct);
        var desc = q.Dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        var ordered = q.Sort switch
        {
            "aksi" => desc ? qb.OrderByDescending(x => x.l.LogMencatat) : qb.OrderBy(x => x.l.LogMencatat),
            "target" => desc ? qb.OrderByDescending(x => x.l.LogTarget) : qb.OrderBy(x => x.l.LogTarget),
            "waktu" => desc ? qb.OrderByDescending(x => x.l.LogCreatedat) : qb.OrderBy(x => x.l.LogCreatedat),
            "pelaku" => desc
                ? qb.OrderByDescending(x => x.p != null ? x.p.PenggunaEmail : x.l.LogPelaku)
                : qb.OrderBy(x => x.p != null ? x.p.PenggunaEmail : x.l.LogPelaku),
            _ => qb.OrderByDescending(x => x.l.LogCreatedat),
        };
        var items = await ordered.Skip((page - 1) * limit).Take(limit)
            .Select(x => new LogView(x.l.LogCode,
                x.p != null ? x.p.PenggunaEmail : null!,
                x.l.LogMencatat, x.l.LogOldvalue, x.l.LogNewvalue, x.l.LogTarget, x.l.LogCreatedat))
            .ToListAsync(ct);
        return new PageResult<LogView>
        { List = items, Total = total, Page = page, Limit = limit,
          TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit) };
    }

    public async Task<LogView?> DetailLogAsync(string code, CancellationToken ct)
        => await (from l in _db.MejaLog
                  join p in _db.MejaPengguna on l.LogPelaku equals p.PenggunaCode into ps
                  from p in ps.DefaultIfEmpty()
                  where l.LogCode == code && l.LogSoftdeleted == 0
                  select new LogView(l.LogCode,
                      p != null ? p.PenggunaEmail : null!,
                      l.LogMencatat, l.LogOldvalue, l.LogNewvalue, l.LogTarget, l.LogCreatedat))
            .FirstOrDefaultAsync(ct);

    // ----------Dashboard----------
    public async Task<DashboardView> DashboardAsync(CancellationToken ct)
    {
        var super = await _otoritas.IsSuperuserAsync(ct);
        var userToko = await _otoritas.UserTokoCodeAsync(ct);

        if (super)
        {
            var tokoAll = await _db.MejaToko.CountAsync(t => t.TokoSoftdeleted == 0, ct);
            var penggunaAll = await _db.MejaPengguna.CountAsync(p => p.PenggunaSoftdeleted == 0, ct);
            var biodataAll = await _db.MejaBiodata.CountAsync(b => b.BiodataSoftdeleted == 0, ct);
            var jabatanAll = await _db.MejaJabatan.CountAsync(j => j.JabatanSoftdeleted == 0, ct);
            var targetAll = await _db.MejaTarget.CountAsync(t => t.TargetSoftdeleted == 0, ct);

            IQueryable<MejaKeuangan> kq = _db.MejaKeuangan.AsNoTracking().Where(x => x.KeuanganSoftdeleted == 0);
            var rows = await kq.ToListAsync(ct);

            var perTempat = rows
                .GroupBy(x => x.KeuanganTempat)
                .Select(g => new SaldoTempat(g.Sum(r => Arah(r.KeuanganStatus, r.KeuanganNominal)), g.Key))
                .OrderByDescending(x => x.Saldo)
                .ToList();

            return new DashboardView(tokoAll, penggunaAll, biodataAll, jabatanAll, targetAll,
                rows.Sum(r => Arah(r.KeuanganStatus, r.KeuanganNominal)), perTempat);
        }

        if (string.IsNullOrEmpty(userToko))
            return new DashboardView(0, 0, 0, 0, 0, 0, new List<SaldoTempat>());

        var biodataToko = await _db.MejaBiodata
            .CountAsync(b => b.BiodataSoftdeleted == 0 && b.TokoCode == userToko, ct);
        var penggunaViaBiodata = await _db.MejaBiodata
            .Where(b => b.BiodataSoftdeleted == 0 && b.TokoCode == userToko)
            .Select(b => b.PenggunaCode).Distinct().CountAsync(ct);
        var jabatanToko = await _db.MejaJabatan
            .Where(j => j.JabatanSoftdeleted == 0
                && _db.MejaBiodata.Any(b => b.BiodataSoftdeleted == 0 && b.TokoCode == userToko && b.JabatanCode == j.JabatanCode))
            .CountAsync(ct);
        var target = await _db.MejaTarget.CountAsync(t => t.TargetSoftdeleted == 0, ct);

        var kq2 = _db.MejaKeuangan.AsNoTracking().Where(x => x.KeuanganSoftdeleted == 0 && x.TokoCode == userToko);
        var rows2 = await kq2.ToListAsync(ct);
        var perTempat2 = rows2
            .GroupBy(x => x.KeuanganTempat)
            .Select(g => new SaldoTempat(g.Sum(r => Arah(r.KeuanganStatus, r.KeuanganNominal)), g.Key))
            .OrderByDescending(x => x.Saldo)
            .ToList();

        return new DashboardView(1, penggunaViaBiodata, biodataToko, jabatanToko, target,
            rows2.Sum(r => Arah(r.KeuanganStatus, r.KeuanganNominal)), perTempat2);
    }

    private static decimal Arah(string status, decimal nominal) => status switch
    {
        KeuanganStatus.Masuk => nominal,
        KeuanganStatus.Keluar => -nominal,
        KeuanganStatus.Hilang => -nominal,
        _ => 0m,
    };
}

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }
    public ApiException(int status, string code, string message) : base(message)
    { StatusCode = status; Code = code; }
    public ApiException(int status, string code) : this(status, code, code) { }
}