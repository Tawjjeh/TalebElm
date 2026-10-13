

using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests
{
    public class UserEntityTests
    {
        [Fact]
        public void User_ShouldHaveEmptyConstructor() { }

        [Fact]
        public void NewUser_ShouldDefaultToStudentRole()
        {
            var user = new User();

            Assert.Equal(UserRole.Student, user.Role);
        }

        [Fact]
        public void User_ShouldStorePasswordHash()
        {
            var user = new User { PasswordHash = "hashed-value" };

            Assert.Equal("hashed-value", user.PasswordHash);
        }
    }
}
