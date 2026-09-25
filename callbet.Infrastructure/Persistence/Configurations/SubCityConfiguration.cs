using callbet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace callbet.Infrastructure.Persistence.Configurations
{
    public class SubCityConfiguration : IEntityTypeConfiguration<SubCity>
    {
        public void Configure(EntityTypeBuilder<SubCity> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).ValueGeneratedOnAdd();
            builder.Property(s => s.Name).IsRequired().HasMaxLength(100);

            builder.HasMany(s => s.Neighborhoods)
                   .WithOne(n => n.SubCity)
                   .HasForeignKey(n => n.SubCityId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
