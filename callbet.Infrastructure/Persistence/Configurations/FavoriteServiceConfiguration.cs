using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class FavoriteServiceConfiguration : IEntityTypeConfiguration<FavoriteService>
    {
        public void Configure(EntityTypeBuilder<FavoriteService> builder)
        {
            builder.HasKey(fs => fs.Id);
            builder.HasIndex(fs => new { fs.CustomerId, fs.ServiceId }).IsUnique();
        }
    }
}