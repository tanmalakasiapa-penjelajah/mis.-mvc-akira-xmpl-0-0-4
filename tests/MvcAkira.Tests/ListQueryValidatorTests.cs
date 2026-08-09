using MvcAkira.Shared.Contracts;
using MvcAkira.Shared.Services;

namespace MvcAkira.Tests;

public class ListQueryValidatorTests
{
    [Theory]
    [InlineData(5, true)]
    [InlineData(25, true)]
    [InlineData(50, true)]
    [InlineData(75, true)]
    [InlineData(100, true)]
    [InlineData(10, false)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1000, false)]
    public void Normalize_LimitYangDiizinkan(int limit, bool expected)
    {
        var q = new ListQuery { Limit = limit };
        var (_, _, ok) = ListQueryValidator.Normalize(q);
        Assert.Equal(expected, ok);
    }

    [Fact]
    public void Normalize_PageDibawahSatu_DikembalikanKeSatu()
    {
        var q = new ListQuery { Page = 0, Limit = 25 };
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        Assert.True(ok);
        Assert.Equal(1, page);
        Assert.Equal(25, limit);
    }

    [Fact]
    public void Normalize_LimitTidakDiizinkan_ReturnPageSatuDanLimitNol()
    {
        var q = new ListQuery { Page = 3, Limit = 7 };
        var (page, limit, ok) = ListQueryValidator.Normalize(q);
        Assert.False(ok);
        Assert.Equal(1, page);
        Assert.Equal(0, limit);
    }
}