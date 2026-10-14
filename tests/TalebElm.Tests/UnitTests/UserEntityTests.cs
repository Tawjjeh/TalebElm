

using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests
{
    public class UserEntityTests
    {
        [Fact]
        public void NewUser_ShouldDefaultToStudentRole()
        {
            var user = new User();

            Assert.Equal(UserRole.Student, user.Role);
        }
    }
}
