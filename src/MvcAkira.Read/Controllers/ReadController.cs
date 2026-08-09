using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcAkira.Shared.Contracts;
using MvcAkira.Shared.Security;
using MvcAkira.Shared.Services;

namespace MvcAkira.Read.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ReadController : ControllerBase
{
    private readonly BacaService _baca;
    private readonly OtoritasService _otoritas;

    public ReadController(BacaService baca, OtoritasService otoritas)
    {
        _baca = baca;
        _otoritas = otoritas;
    }

    // ---------- Dashboard ----------
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardView>> Dashboard(CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Keuangan, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.DashboardAsync(ct));
    }

    // ---------- TOKO ----------
    [HttpGet("meja_toko")]
    public async Task<ActionResult<PageResult<TokoView>>> Toko([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Toko, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.ListToko(q, ct));
    }

    [HttpGet("meja_toko/detail")]
    public async Task<ActionResult<TokoView>> TokoDetail([FromQuery] string code, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Toko, ct);
        if (!boleh.Boleh) return Forbid();
        var x = await _baca.DetailTokoAsync(code, ct);
        return x is null ? NotFound() : Ok(x);
    }

    [HttpGet("meja_toko/trash")]
    public async Task<ActionResult<PageResult<TokoView>>> TokoTrash([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Toko, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.TrashToko(q, ct));
    }

    // ---------- TRASH (semua tabel) ----------
    [HttpGet("meja_jabatan/trash")]
    public async Task<ActionResult<PageResult<JabatanView>>> JabatanTrash([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Jabatan, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.TrashJabatan(q, ct));
    }

    // ---------- JABATAN ----------
    [HttpGet("meja_jabatan")]
    public async Task<ActionResult<PageResult<JabatanView>>> Jabatan([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Jabatan, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.ListJabatan(q, ct));
    }

    [HttpGet("meja_jabatan/detail")]
    public async Task<ActionResult<JabatanView>> JabatanDetail([FromQuery] string code, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Jabatan, ct);
        if (!boleh.Boleh) return Forbid();
        var x = await _baca.DetailJabatanAsync(code, ct);
        return x is null ? NotFound() : Ok(x);
    }

    // ---------- TARGET ----------
    [HttpGet("meja_target")]
    public async Task<ActionResult<PageResult<TargetView>>> Target([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Target, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.ListTarget(q, ct));
    }

    [HttpGet("meja_target/detail")]
    public async Task<ActionResult<TargetView>> TargetDetail([FromQuery] string code, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Target, ct);
        if (!boleh.Boleh) return Forbid();
        var x = await _baca.DetailTargetAsync(code, ct);
        return x is null ? NotFound() : Ok(x);
    }

    [HttpGet("meja_target/trash")]
    public async Task<ActionResult<PageResult<TargetView>>> TargetTrash([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Target, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.TrashTarget(q, ct));
    }

    // ---------- PENGGUNA ----------
    [HttpGet("meja_pengguna")]
    public async Task<ActionResult<PageResult<PenggunaView>>> Pengguna([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Pengguna, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.ListPengguna(q, ct));
    }

    [HttpGet("meja_pengguna/detail")]
    public async Task<ActionResult<PenggunaView>> PenggunaDetail([FromQuery] string code, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Pengguna, ct);
        if (!boleh.Boleh) return Forbid();
        var x = await _baca.DetailPenggunaAsync(code, ct);
        return x is null ? NotFound() : Ok(x);
    }

    [HttpGet("meja_pengguna/trash")]
    public async Task<ActionResult<PageResult<PenggunaView>>> PenggunaTrash([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Pengguna, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.TrashPengguna(q, ct));
    }

    // ---------- BIODATA ----------
    [HttpGet("meja_biodata")]
    public async Task<ActionResult<PageResult<BiodataView>>> Biodata([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Biodata, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.ListBiodata(q, ct));
    }

    [HttpGet("meja_biodata/detail")]
    public async Task<ActionResult<BiodataView>> BiodataDetail([FromQuery] string code, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Biodata, ct);
        if (!boleh.Boleh) return Forbid();
        var x = await _baca.DetailBiodataAsync(code, ct);
        return x is null ? NotFound() : Ok(x);
    }

    [HttpGet("meja_biodata/trash")]
    public async Task<ActionResult<PageResult<BiodataView>>> BiodataTrash([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Biodata, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.TrashBiodata(q, ct));
    }

    // ---------- HAK AKSES ----------
    [HttpGet("meja_hakakses")]
    public async Task<ActionResult<PageResult<HakaksesView>>> Hakakses([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Hakakses, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.ListHakakses(q, ct));
    }

    [HttpGet("meja_hakakses/detail")]
    public async Task<ActionResult<HakaksesView>> HakaksesDetail([FromQuery] string code, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Hakakses, ct);
        if (!boleh.Boleh) return Forbid();
        var x = await _baca.DetailHakaksesAsync(code, ct);
        return x is null ? NotFound() : Ok(x);
    }

    [HttpGet("meja_hakakses/trash")]
    public async Task<ActionResult<PageResult<HakaksesView>>> HakaksesTrash([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Hakakses, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.TrashHakakses(q, ct));
    }

    // ---------- KEUANGAN ----------
    [HttpGet("meja_keuangan")]
    public async Task<ActionResult<PageResult<KeuanganView>>> Keuangan([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Keuangan, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.ListKeuangan(q, ct));
    }

    [HttpGet("meja_keuangan/detail")]
    public async Task<ActionResult<KeuanganView>> KeuanganDetail([FromQuery] string code, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Keuangan, ct);
        if (!boleh.Boleh) return Forbid();
        var x = await _baca.DetailKeuanganAsync(code, ct);
        return x is null ? NotFound() : Ok(x);
    }

    [HttpGet("meja_keuangan/trash")]
    public async Task<ActionResult<PageResult<KeuanganView>>> KeuanganTrash([FromQuery] ListQuery q, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Keuangan, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.TrashKeuangan(q, ct));
    }

    // ---------- LOG ----------
    [HttpGet("meja_log")]
    public async Task<ActionResult<PageResult<LogView>>> Log([FromQuery] ListQuery q,
        [FromQuery] string? dari, [FromQuery] string? sampai, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Log, ct);
        if (!boleh.Boleh) return Forbid();
        return Ok(await _baca.ListLog(q, dari, sampai, ct));
    }

    [HttpGet("meja_log/detail")]
    public async Task<ActionResult<LogView>> LogDetail([FromQuery] string code, CancellationToken ct)
    {
        var boleh = await _baca.CekBacaAsync(CoreTables.Log, ct);
        if (!boleh.Boleh) return Forbid();
        var x = await _baca.DetailLogAsync(code, ct);
        return x is null ? NotFound() : Ok(x);
    }
}