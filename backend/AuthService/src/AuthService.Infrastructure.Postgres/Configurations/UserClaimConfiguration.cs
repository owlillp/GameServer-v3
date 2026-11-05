using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Postgres.Configurations;

internal sealed class UserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("account_claims");

        builder.Property(uc => uc.Id).HasColumnName("id");
        builder.Property(uc => uc.UserId).HasColumnName("account_id");
        builder.Property(uc => uc.ClaimType).HasColumnName("claim_type");
        builder.Property(uc => uc.ClaimValue).HasColumnName("claim_value");
    }
}