using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcAkira.Shared.Services;

namespace MvcAkira.Write.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class WriteController : ControllerBase
{
    private readonly TulisService _tulis;

    public WriteController(TulisService tulis) => _tulis = tulis;

    // ---------- TOKO ----------
    [HttpPost("meja_toko")]
    public async Task<IActionResult> TokoCreate([FromBody] TokoBody body, CancellationToken ct)
    {
        var r = await _tulis.CreateTokoAsync(body.Name!, body.Address!, body.Email!, body.Phone!, ct);
        return Res(r);
    }

    [HttpPut("meja_toko")]
    public async Task<IActionResult> TokoUpdate([FromBody] TokoBody body, CancellationToken ct)
    {
        var r = await _tulis.UpdateTokoAsync(body.Code!, body.Name, body.Address, body.Email, body.Phone, ct);
        return Res(r);
    }

    [HttpPost("meja_toko/soft-delete")]
    public async Task<IActionResult> TokoSoftDelete([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.SoftDeleteTokoAsync(body.Code, ct));

    [HttpPost("meja_toko/restore")]
    public async Task<IActionResult> TokoRestore([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.RestoreTokoAsync(body.Code, ct));

    [HttpDelete("meja_toko/permanent")]
    public async Task<IActionResult> TokoPermanent([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.PermanentTokoAsync(body.Code, ct));

    // ---------- JABATAN ----------
    [HttpPost("meja_jabatan")]
    public async Task<IActionResult> JabatanCreate([FromBody] JabatanBody body, CancellationToken ct)
        => Res(await _tulis.CreateJabatanAsync(body.Name!, ct));

    [HttpPut("meja_jabatan")]
    public async Task<IActionResult> JabatanUpdate([FromBody] JabatanBody body, CancellationToken ct)
        => Res(await _tulis.UpdateJabatanAsync(body.Code!, body.Name!, ct));

    [HttpPost("meja_jabatan/soft-delete")]
    public async Task<IActionResult> JabatanSoftDelete([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.SoftDeleteJabatanAsync(body.Code, ct));

    [HttpPost("meja_jabatan/restore")]
    public async Task<IActionResult> JabatanRestore([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.RestoreJabatanAsync(body.Code, ct));

    [HttpDelete("meja_jabatan/permanent")]
    public async Task<IActionResult> JabatanPermanent([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.PermanentJabatanAsync(body.Code, ct));

    // ---------- TARGET ----------
    [HttpPost("meja_target")]
    public async Task<IActionResult> TargetCreate([FromBody] TargetBody body, CancellationToken ct)
        => Res(await _tulis.CreateTargetAsync(body.Name!, body.Keterangan!, ct));

    [HttpPut("meja_target")]
    public async Task<IActionResult> TargetUpdate([FromBody] TargetBody body, CancellationToken ct)
        => Res(await _tulis.UpdateTargetAsync(body.Code!, body.Name!, body.Keterangan, ct));

    [HttpPost("meja_target/soft-delete")]
    public async Task<IActionResult> TargetSoftDelete([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.SoftDeleteTargetAsync(body.Code, ct));

    [HttpPost("meja_target/restore")]
    public async Task<IActionResult> TargetRestore([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.RestoreTargetAsync(body.Code, ct));

    [HttpDelete("meja_target/permanent")]
    public async Task<IActionResult> TargetPermanent([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.PermanentTargetAsync(body.Code, ct));

    // ---------- PENGGUNA ----------
    [HttpPost("meja_pengguna")]
    public async Task<IActionResult> PenggunaCreate([FromBody] PenggunaBody body, CancellationToken ct)
        => Res(await _tulis.CreatePenggunaAsync(body.Email!, body.Password!, ct));

    [HttpPut("meja_pengguna")]
    public async Task<IActionResult> PenggunaUpdate([FromBody] PenggunaBody body, CancellationToken ct)
        => Res(await _tulis.UpdatePenggunaAsync(body.Code!, body.Email!, body.Nonaktif, ct));

    [HttpPost("meja_pengguna/nonaktif")]
    public async Task<IActionResult> PenggunaNonaktif([FromBody] NonaktifBody body, CancellationToken ct)
        => Res(await _tulis.SetNonaktifPenggunaAsync(body.Code!, body.Nonaktif, ct));

    [HttpPost("meja_pengguna/reset-password")]
    public async Task<IActionResult> PenggunaReset([FromBody] ResetBody body, CancellationToken ct)
        => Res(await _tulis.ResetPasswordPenggunaAsync(body.Code!, body.Password!, ct));

    [HttpPost("meja_pengguna/soft-delete")]
    public async Task<IActionResult> PenggunaSoftDelete([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.SoftDeletePenggunaAsync(body.Code, ct));

    [HttpPost("meja_pengguna/restore")]
    public async Task<IActionResult> PenggunaRestore([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.RestorePenggunaAsync(body.Code, ct));

    [HttpDelete("meja_pengguna/permanent")]
    public async Task<IActionResult> PenggunaPermanent([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.PermanentPenggunaAsync(body.Code, ct));

    // ---------- BIODATA ----------
    [HttpPost("meja_biodata")]
    public async Task<IActionResult> BiodataCreate([FromBody] BiodataBody body, CancellationToken ct)
        => Res(await _tulis.CreateBiodataAsync(body.PenggunaCode!, body.TokoCode!, body.JabatanCode!,
            body.Fullname!, body.Born!, body.Address!, body.Phone!, ct));

    [HttpPut("meja_biodata")]
    public async Task<IActionResult> BiodataUpdate([FromBody] BiodataBody body, CancellationToken ct)
        => Res(await _tulis.UpdateBiodataAsync(body.Code!, body.Fullname!, body.Born!, body.Address!, body.Phone!, ct));

    [HttpPost("meja_biodata/soft-delete")]
    public async Task<IActionResult> BiodataSoftDelete([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.SoftDeleteBiodataAsync(body.Code, ct));

    [HttpPost("meja_biodata/restore")]
    public async Task<IActionResult> BiodataRestore([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.RestoreBiodataAsync(body.Code, ct));

    [HttpDelete("meja_biodata/permanent")]
    public async Task<IActionResult> BiodataPermanent([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.PermanentBiodataAsync(body.Code, ct));

    // ---------- HAK AKSES ----------
    [HttpPost("meja_hakakses")]
    public async Task<IActionResult> HakaksesUpsert([FromBody] HakaksesBody body, CancellationToken ct)
        => Res(await _tulis.UpsertHakaksesAsync(body.PenggunaCode!, body.TargetCode!,
            body.Read, body.Create, body.Update, body.Delete, body.Login, ct));

    [HttpPost("meja_hakakses/soft-delete")]
    public async Task<IActionResult> HakaksesSoftDelete([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.SoftDeleteHakaksesAsync(body.Code, ct));

    [HttpPost("meja_hakakses/restore")]
    public async Task<IActionResult> HakaksesRestore([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.RestoreHakaksesAsync(body.Code, ct));

    [HttpDelete("meja_hakakses/permanent")]
    public async Task<IActionResult> HakaksesPermanent([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.PermanentHakaksesAsync(body.Code, ct));

    // ---------- KEUANGAN ----------
    [HttpPost("meja_keuangan")]
    public async Task<IActionResult> KeuanganCreate([FromBody] KeuanganBody body, CancellationToken ct)
        => Res(await _tulis.CreateKeuanganAsync(body.PenggunaCode!, body.TokoCode!,
            body.Nominal, body.Judul!, body.Deskripsi!, body.Status!, body.Tempat!, body.Waktucatat!, ct));

    [HttpPut("meja_keuangan")]
    public async Task<IActionResult> KeuanganUpdate([FromBody] KeuanganBody body, CancellationToken ct)
        => Res(await _tulis.UpdateKeuanganAsync(body.Code!, body.Judul, body.Deskripsi,
            body.Status, body.Tempat, body.Waktucatat, ct));

    [HttpPost("meja_keuangan/soft-delete")]
    public async Task<IActionResult> KeuanganSoftDelete([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.SoftDeleteKeuanganAsync(body.Code, ct));

    [HttpPost("meja_keuangan/restore")]
    public async Task<IActionResult> KeuanganRestore([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.RestoreKeuanganAsync(body.Code, ct));

    [HttpDelete("meja_keuangan/permanent")]
    public async Task<IActionResult> KeuanganPermanent([FromBody] KodeBody body, CancellationToken ct)
        => Res(await _tulis.PermanentKeuanganAsync(body.Code, ct));

    private IActionResult Res(HasilTulis r)
        => r.Ok ? Ok(new { kode = r.Kode }) : StatusCode(r.Status, new { pesan = r.Pesan, kode = r.Kode ?? r.Pesan });
}

public record TokoBody(string? Code, string? Name, string? Address, string? Email, string? Phone);
public record KodeBody(string Code);
public record JabatanBody(string? Code, string? Name);
public record TargetBody(string? Code, string? Name, string? Keterangan);
public record NonaktifBody(string? Code, int Nonaktif);
public record ResetBody(string? Code, string? Password);
public record PenggunaBody(string? Code, string? Email, string? Password, int? Nonaktif);
public record BiodataBody(string? Code, string? PenggunaCode, string? TokoCode, string? JabatanCode, string? Fullname,
    string? Born, string? Address, string? Phone);
public record HakaksesBody(string? PenggunaCode, string? TargetCode, int Read, int Create, int Update, int Delete, int Login);
public record KeuanganBody(string? Code, string? PenggunaCode, string? TokoCode, decimal Nominal,
    string? Judul, string? Deskripsi, string? Status, string? Tempat, string? Waktucatat);