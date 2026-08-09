using MvcAkira.Shared.Enums;

namespace MvcAkira.Tests;

public class KeuanganValidatorTests
{
    [Theory]
    [InlineData(KeuanganStatus.Masuk, true)]
    [InlineData(KeuanganStatus.Keluar, true)]
    [InlineData(KeuanganStatus.Hilang, true)]
    [InlineData(KeuanganStatus.Pindah, true)]
    [InlineData("sembarang", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void KeuanganStatus_IsValid(string? value, bool expected)
        => Assert.Equal(expected, KeuanganStatus.IsValid(value));

    [Theory]
    [InlineData(KeuanganTempat.Tunai, true)]
    [InlineData(KeuanganTempat.Bank, true)]
    [InlineData(KeuanganTempat.Ewallet, true)]
    [InlineData(KeuanganTempat.Others, true)]
    [InlineData("gold", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void KeuanganTempat_IsValid(string? value, bool expected)
        => Assert.Equal(expected, KeuanganTempat.IsValid(value));
}