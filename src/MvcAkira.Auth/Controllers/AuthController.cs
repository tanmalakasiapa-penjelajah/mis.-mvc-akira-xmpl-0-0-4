using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcAkira.Auth.Requests;
using MvcAkira.Auth.Services;
using MvcAkira.Shared.Contracts;
using MvcAkira.Shared.Data;
using MvcAkira.Shared.Entities;
using MvcAkira.Shared.Enums;
using MvcAkira.Shared.Security;

namespace MvcAkira.Auth.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AkiraDbContext _db;
    private readonly IJwtService _jwt;
    private readonly OtoritasService _otoritas;
    private readonly LogService _log;

    public AuthController(AkiraDbContext db, IJwtService jwt, OtoritasService otoritas, LogService log)
    {
        _db = db;
        _jwt = jwt;
        _otoritas = otoritas;
        _log = log;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Login(LoginRequest req, CancellationToken ct)
    {
        var pengguna = await _db.MejaPengguna
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PenggunaEmail == req.Email && p.PenggunaSoftdeleted == 0, ct);

        if (pengguna is null || !BCrypt.Net.BCrypt.Verify(req.Password, pengguna.PenggunaPassword))
            return Unauthorized(new AuthResult(false, Error: "email atau kata sandi salah"));

        if (pengguna.PenggunaNonaktif == 1)
            return Unauthorized(new AuthResult(false, Error: "akun tidak aktif"));

        // Cek hak login
        var bolehLogin = await _otoritas.BolehLoginAsync(pengguna.PenggunaCode, ct);
        if (!bolehLogin)
            return Unauthorized(new AuthResult(false, Error: "akun belum memiliki hak login"));

        var aid = await GetAksesContextAsync(pengguna, ct);
        var token = _jwt.Generate(pengguna, aid.Nama, aid.TokoCode, aid.TokoName!, aid.Jabatan!, aid.IsSuperuser);

        await _log.CatatAsync(LogAksi.Login, LogService.Target(CoreTables.Pengguna, pengguna.PenggunaCode),
            "-", pengguna.PenggunaCode, ct);

        return Ok(new AuthResult(true, token, pengguna.PenggunaEmail, aid.Nama, aid.TokoName, aid.IsSuperuser));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Register(RegisterRequest req, CancellationToken ct)
    {
        if (await _db.MejaPengguna.AnyAsync(p => p.PenggunaEmail == req.Email, ct))
            return Conflict(new { message = "email sudah terdaftar" });

        var now = DateStamp.Now();
        var user = new MejaPengguna
        {
            PenggunaCode = CodeGenerator.Next("meja_pengguna"),
            PenggunaEmail = req.Email,
            PenggunaPassword = BCrypt.Net.BCrypt.HashPassword(req.Password),
            PenggunaNonaktif = 0,
            PenggunaSoftdeleted = 0,
            PenggunaCreatedat = now,
            PenggunaUpdatedat = now,
        };
        _db.MejaPengguna.Add(user);
        await _db.SaveChangesAsync(ct);

        await _log.CatatAsync(LogAksi.Create, LogService.Target(CoreTables.Pengguna, user.PenggunaCode),
            "-", user.PenggunaEmail, ct);

        // Belum ada hak akses -> belum bisa login sampai admin grant.
        return Ok(new AuthResult(true, Error: "akun dibuat, tunggu admin memberikan hak akses"));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout(CancellationToken ct)
    {
        await _log.CatatAsync(LogAksi.Logout, LogService.Target(CoreTables.Pengguna, User.Identity?.Name ?? "-"),
            "-", "-", ct);
        return Ok(new { message = "berhasil logout" });
    }

    private async Task<(string? Nama, string? TokoCode, string? TokoName, string? Jabatan, bool IsSuperuser)>
        GetAksesContextAsync(MejaPengguna pengguna, CancellationToken ct)
    {
        var b = await _db.MejaBiodata.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PenggunaCode == pengguna.PenggunaCode && x.BiodataSoftdeleted == 0, ct);
        if (b is null) return (null, null, null, null, false);

        var toko = await _db.MejaToko.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokoCode == b.TokoCode && t.TokoSoftdeleted == 0, ct);
        var jabatan = await _db.MejaJabatan.AsNoTracking()
            .FirstOrDefaultAsync(j => j.JabatanCode == b.JabatanCode && j.JabatanSoftdeleted == 0, ct);

        var isSuper = jabatan?.JabatanName == JabatanNama.Developer;
        return (b.BiodataFullname, b.TokoCode, toko?.TokoName, jabatan?.JabatanName, isSuper);
    }
}