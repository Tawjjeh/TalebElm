using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;

namespace TalebElm.Domain.Interfaces;

public interface IUserExternalAccountRepository : IRepository<UserExternalAccount>
{
    Task<UserExternalAccount?> GetByUserAndPlatformAsync(Guid userId, ExternalPlatform platform, CancellationToken ct = default);
    Task<UserExternalAccount?> GetByPlatformAndHandleAsync(ExternalPlatform platform, string handle, CancellationToken ct = default);
}
