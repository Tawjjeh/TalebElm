using TalebElm.Application.Services.Abstraction;
using TalebElm.Application.Services.Concrete;

namespace TalebElm.Tests.UnitTests;

public class PasswordHasherTests
{
    private readonly IPasswordHasher _sut = new PasswordHasher();

    [Fact]
    public void HashPassword_SamePasswordTwice_ProducesDifferentHashesThatBothVerify()
    {
        var first = _sut.HashPassword("Correct-Horse-1");
        var second = _sut.HashPassword("Correct-Horse-1");

        Assert.NotEqual(first, second);
        Assert.True(_sut.VerifyHashedPassword(first, "Correct-Horse-1"));
        Assert.True(_sut.VerifyHashedPassword(second, "Correct-Horse-1"));
    }

    [Fact]
    public void VerifyHashedPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.HashPassword("Correct-Horse-1");

        Assert.False(_sut.VerifyHashedPassword(hash, "correct-horse-1"));
        Assert.False(_sut.VerifyHashedPassword(hash, "Something-Else-2"));
        Assert.False(_sut.VerifyHashedPassword(hash, ""));
    }

    [Fact]
    public void HashPassword_DoesNotContainOrEqualTheRawPassword()
    {
        const string password = "Correct-Horse-1";

        var hash = _sut.HashPassword(password);

        Assert.NotEqual(password, hash);
        Assert.DoesNotContain(password, hash);
    }
}
