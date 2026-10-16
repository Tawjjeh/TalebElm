namespace TalebElm.Tests.UnitTests
{
    public class UserDtoTests
    {
        [Fact]
        public void CreateUserRequest_ShouldStoreValues() { }

        [Fact]
        public void NoDto_ShouldExposePasswordOrHash()
        {
            var dtoTypes = typeof(TalebElm.Application.DTOs.UserResponse).Assembly
                .GetTypes()
                .Where(t => t.IsClass && t.Namespace == typeof(TalebElm.Application.DTOs.UserResponse).Namespace);

            foreach (var dtoType in dtoTypes)
            {
                Assert.DoesNotContain(dtoType.GetProperties(), p =>
                    p.Name.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("Hash", StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
