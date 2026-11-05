using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Postgres.Configurations;

internal sealed class UserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("account_logins");

        builder.Property(ul => ul.LoginProvider).HasColumnName("login_provider");
        builder.Property(ul => ul.ProviderKey).HasColumnName("provider_key");
        builder.Property(ul => ul.ProviderDisplayName).HasColumnName("provider_display_name");
        builder.Property(ul => ul.UserId).HasColumnName("account_id");
    }
}