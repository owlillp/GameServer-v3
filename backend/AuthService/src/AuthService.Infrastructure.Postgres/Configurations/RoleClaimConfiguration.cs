using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Postgres.Configurations;

internal sealed class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("role_claims");

        builder.Property(rc => rc.Id).HasColumnName("id");
        builder.Property(rc => rc.RoleId).HasColumnName("role_id");
        builder.Property(rc => rc.ClaimType).HasColumnName("claim_type");
        builder.Property(rc => rc.ClaimValue).HasColumnName("claim_value");
    }
}