using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProfileService.Domain.Players;

namespace ProfileService.Infrastructure.Postgres.Configurations;

internal sealed class PlayerProfileConfiguration : IEntityTypeConfiguration<PlayerProfile>
{
    public void Configure(EntityTypeBuilder<PlayerProfile> builder)
    {
        builder.ToTable("player_profiles");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.Nickname)
            .HasColumnName("nickname")
            .HasConversion(n => n.Value, s => Nickname.Create(s).Value)
            .HasMaxLength(PlayerProfileConstants.NICKNAME_MAX_LENGTH);

        builder.Property(p => p.Region)
            .HasColumnName("region")
            .IsRequired(false)
            .HasConversion(r => r!.Value, s => Region.Create(s).Value)
            .HasMaxLength(PlayerProfileConstants.REGION_MAX_LENGTH);

        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        builder.HasKey(p => p.Id);

        builder.HasIndex(p => p.Nickname)
            .IsUnique()
            .HasDatabaseName("uq_profiles_nickname");
    }
}
