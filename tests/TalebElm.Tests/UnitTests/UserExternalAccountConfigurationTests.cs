using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using TalebElm.Domain.Entities;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class UserExternalAccountConfigurationTests : SqliteTestBase
{
    [Fact]
    public void UserExternalAccount_PrimaryKey_ShouldBeId()
    {
        var context = DbContext;
        var entityType = context.Model.FindEntityType(typeof(UserExternalAccount));

        Assert.NotNull(entityType);
        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal(nameof(UserExternalAccount.Id), primaryKey.Properties[0].Name);
    }

    [Fact]
    public void UserExternalAccount_Properties_ShouldHaveConfiguredLengthsAndNullability()
    {
        var context = DbContext;
        var entityType = context.Model.FindEntityType(typeof(UserExternalAccount));

        Assert.NotNull(entityType);

        var handleProp = entityType.FindProperty(nameof(UserExternalAccount.Handle));
        Assert.NotNull(handleProp);
        Assert.False(handleProp.IsNullable);
        Assert.Equal(64, handleProp.GetMaxLength());

        var externalIdProp = entityType.FindProperty(nameof(UserExternalAccount.ExternalNumericId));
        Assert.NotNull(externalIdProp);
        Assert.True(externalIdProp.IsNullable);
        Assert.Equal(64, externalIdProp.GetMaxLength());

        var tokenProp = entityType.FindProperty(nameof(UserExternalAccount.VerificationToken));
        Assert.NotNull(tokenProp);
        Assert.True(tokenProp.IsNullable);
        Assert.Equal(64, tokenProp.GetMaxLength());
    }

    [Fact]
    public void UserExternalAccount_ForeignKeyToUser_ShouldUseRestrictDelete()
    {
        var context = DbContext;
        var entityType = context.Model.FindEntityType(typeof(UserExternalAccount));

        Assert.NotNull(entityType);

        var fk = Assert.Single(
            entityType.GetForeignKeys(),
            f => f.PrincipalEntityType.ClrType == typeof(User)
                 && f.Properties.Any(p => p.Name == nameof(UserExternalAccount.UserId)));

        Assert.Equal(nameof(UserExternalAccount.UserId), Assert.Single(fk.Properties).Name);
        Assert.Equal(nameof(User.Id), Assert.Single(fk.PrincipalKey.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void UserExternalAccount_Indexes_ShouldBeConfiguredWithDecision33Rules()
    {
        var context = DbContext;
        var entityType = context.Model.FindEntityType(typeof(UserExternalAccount));

        Assert.NotNull(entityType);
        var indexes = entityType.GetIndexes().ToList();

        // 1. UNIQUE(UserId, Platform)
        var userPlatformIndex = indexes.SingleOrDefault(i =>
            i.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(UserExternalAccount.UserId),
                nameof(UserExternalAccount.Platform)
            }));
        Assert.NotNull(userPlatformIndex);
        Assert.True(userPlatformIndex.IsUnique);
        Assert.Null(userPlatformIndex.GetFilter());

        // 2. UNIQUE(Platform, Handle) filtered where Status = 1 (Verified)
        var platformHandleIndex = indexes.SingleOrDefault(i =>
            i.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(UserExternalAccount.Platform),
                nameof(UserExternalAccount.Handle)
            }));
        Assert.NotNull(platformHandleIndex);
        Assert.True(platformHandleIndex.IsUnique);
        Assert.Equal("Status = 1", platformHandleIndex.GetFilter());

        // 3. UNIQUE(Platform, ExternalNumericId) filtered where ExternalNumericId IS NOT NULL
        var platformExternalIdIndex = indexes.SingleOrDefault(i =>
            i.Properties.Select(p => p.Name).SequenceEqual(new[]
            {
                nameof(UserExternalAccount.Platform),
                nameof(UserExternalAccount.ExternalNumericId)
            }));
        Assert.NotNull(platformExternalIdIndex);
        Assert.True(platformExternalIdIndex.IsUnique);
        Assert.Equal("ExternalNumericId IS NOT NULL", platformExternalIdIndex.GetFilter());
    }

    [Fact]
    public void UserExternalAccount_CheckConstraint_ShouldExistOnHandle()
    {
        var context = DbContext;
        var designTimeModel = context.GetService<IDesignTimeModel>().Model;
        var entityType = designTimeModel.FindEntityType(typeof(UserExternalAccount));

        Assert.NotNull(entityType);
        var checkConstraint = entityType.GetCheckConstraints()
            .SingleOrDefault(c => c.Name == "CK_UserExternalAccounts_Handle_NotEmpty");

        Assert.NotNull(checkConstraint);
        Assert.Equal("length(trim(Handle)) > 0", checkConstraint.Sql);
    }
}
