using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class FavoriteProfessionalConfiguration : IEntityTypeConfiguration<FavoriteProfessional>
    {
        public void Configure(EntityTypeBuilder<FavoriteProfessional> builder)
        {
            builder.HasKey(fp => fp.Id);
            builder.HasIndex(fp => new { fp.CustomerId, fp.ProfessionalProfileId }).IsUnique();
        }
    }
}