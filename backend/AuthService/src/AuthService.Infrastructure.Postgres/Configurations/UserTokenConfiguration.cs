using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Postgres.Configurations;

internal sealed class UserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("account_tokens");

        builder.Property(ut => ut.UserId).HasColumnName("account_id");
        builder.Property(ut => ut.LoginProvider).HasColumnName("login_provider");
        builder.Property(ut => ut.Name).HasColumnName("name");
        builder.Property(ut => ut.Value).HasColumnName("value");
    }
}